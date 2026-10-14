using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using MafRagPoc.Models;
using MafRagPoc.Services;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var settings = configuration.GetSection("AzureOpenAI").Get<AzureOpenAISettings>()
    ?? throw new InvalidOperationException("AzureOpenAI configuration section is missing.");

ValidateSettings(settings);

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
    });
});

var endpoint = new Uri(settings.Endpoint);
var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    ExcludeInteractiveBrowserCredential = true
});

var azureOpenAIClient = new AzureOpenAIClient(endpoint, credential);
var documentPath = Path.Combine(AppContext.BaseDirectory, "Data", "document.txt");
var document =  await DocumentLoader.LoadAsync(documentPath);
var chunker = new DocumentChunker();

var embeddingService = new EmbeddingService(azureOpenAIClient, settings.EmbeddingDeployment);
var vectorSearchService = new VectorSearchService(embeddingService);


Console.WriteLine("Loading and indexing document...");

//var documentText = await loader.LoadAsync(documentPath);
var chunks = await chunker.ChunkAsync(document);
var indexedChunks = new List<DocumentChunk>();

foreach (var chunk in chunks)
{
    var embedding = await embeddingService.GenerateEmbeddingAsync(chunk);
    indexedChunks.Add(new DocumentChunk
    {
        Id = new Guid().ToString(),
        SourceName = "HR Data",
        Text = chunk,
        Embedding = embedding
    });
}

await vectorSearchService.AddAsync(indexedChunks);

Console.WriteLine($"Indexed {indexedChunks.Count} chunk(s) from {documentPath}.");

// Create a text search provider that queries the vector store and
// returns results formatted for the AI context provider.
var textSearchProvider = new TextSearchProvider(
    async (query, cancellationToken) =>
    {
        var results = await vectorSearchService.SearchAsync(query, 3, cancellationToken);
        return results.Select(result => new TextSearchProvider.TextSearchResult
        {
            SourceName = result.Chunk.SourceName,
            SourceLink = result.Chunk.Id,
            Text = result.Chunk.Text,
            RawRepresentation = result
        });
    },
    new TextSearchProviderOptions
    {
        SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke
    },
    loggerFactory);

ChatClient chatClient = azureOpenAIClient.GetChatClient(settings.ChatDeployment);

var agent = chatClient.AsAIAgent(
    instructions: "You are a helpful HR assistant. Answer only from the provided retrieved context. If the answer is not in the context, say that you do not know based on the document.",
    name: "rag-poc-agent",
    description: "Simple retrieval augmented generation sample agent.",
    tools: [],
    clientFactory: innerClient => innerClient.AsBuilder().UseAIContextProviders(textSearchProvider).Build(),
    loggerFactory: loggerFactory,
    services: null);

var session = await agent.CreateSessionAsync();

Console.WriteLine("Ready. Ask a question about the document. Type 'exit' to quit.");

while (true)
{
    Console.WriteLine();
    Console.Write("Question: ");
    var question = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(question))
    {
        continue;
    }

    if (string.Equals(question, "exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    var retrievalResults = await vectorSearchService.SearchAsync(question, 3);

    Console.WriteLine("Top matches:");
    foreach (var result in retrievalResults)
    {
        var preview = result.Chunk.Text.Length > 160
            ? result.Chunk.Text[..160] + "..."
            : result.Chunk.Text;

        Console.WriteLine($"- {result.Chunk.Id} | score={result.Similarity:F4} | {preview}");
    }

    var response = await agent.RunAsync(question, session);
    Console.WriteLine();

    Console.WriteLine("Answer:");
    Console.WriteLine(response.Text);
}

static void ValidateSettings(AzureOpenAISettings settings)
{
    if (string.IsNullOrWhiteSpace(settings.Endpoint) ||
        string.IsNullOrWhiteSpace(settings.ChatDeployment) ||
        string.IsNullOrWhiteSpace(settings.EmbeddingDeployment))
    {
        throw new InvalidOperationException("Azure OpenAI configuration is incomplete. Set Endpoint, ChatDeployment, and EmbeddingDeployment in appsettings.json.");
    }

    if (settings.Endpoint.Contains("your-resource", StringComparison.OrdinalIgnoreCase) ||
        settings.ChatDeployment.Contains("your-chat-deployment", StringComparison.OrdinalIgnoreCase) ||
        settings.EmbeddingDeployment.Contains("your-embedding-deployment", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Azure OpenAI configuration still contains placeholder values. Update appsettings.json before running the sample.");
    }
}

internal sealed class AzureOpenAISettings
{
    public string Endpoint { get; init; } = string.Empty;
    public string ChatDeployment { get; init; } = string.Empty;
    public string EmbeddingDeployment { get; init; } = string.Empty;
}