using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Embeddings;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MafRagPoc.Services
{
    public class EmbeddingService
    {
        private readonly EmbeddingClient _embeddingClient;
        public EmbeddingService(AzureOpenAIClient azureOpenAIClient, string embeddingDeployment)
        {
            //Add Validation for embeddingDeployment and azureOpenAIClient
            if (azureOpenAIClient == null) throw new ArgumentNullException(nameof(azureOpenAIClient));
            if (string.IsNullOrWhiteSpace(embeddingDeployment)) throw new ArgumentNullException(nameof(embeddingDeployment));

            _embeddingClient = azureOpenAIClient.GetEmbeddingClient(embeddingDeployment);
           
        }

        public async Task<IReadOnlyList<float>> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken = default)
        {
            //validate input request
            if (string.IsNullOrWhiteSpace(input)) throw new ArgumentNullException(nameof(input));   
            var response = await _embeddingClient.GenerateEmbeddingAsync(input, cancellationToken: cancellationToken);
            return response.Value.ToFloats().ToArray();
        }

    }
}
