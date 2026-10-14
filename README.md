MafRagPoc - Microsoft Agent Framework RAG POC

What this POC demonstrates
- A simple Retrieval-Augmented Generation (RAG) flow using C# and Azure OpenAI.
- Document loading, chunking, embedding generation, in-memory vector store, vector similarity search.
- A lightweight TextSearchProvider adapter and a simple agent that uses retrieved context to produce grounded answers.

Architecture

INDEXING
document.txt
|
v
Document Loader
|
v
Chunking
|
v
Azure OpenAI Embedding Model
|
v
In-Memory Vector Store

QUERY
User Question
|
v
Question Embedding
|
v
Vector Similarity Search
|
v
Top 3 Document Chunks
|
v
MAF TextSearchProvider (adapter)
|
v
MAF Agent (lightweight)
|
v
Azure OpenAI Chat Model
|
v
Grounded Answer

Project structure
- Data/document.txt - sample document
- Models/DocumentChunk.cs - chunk model
- Services/DocumentLoader.cs - reads file
- Services/DocumentChunker.cs - creates overlapping chunks
- Services/EmbeddingService.cs - wraps Azure OpenAI embeddings
- Services/VectorSearchService.cs - in-memory vector store with cosine similarity
- Services/MafTextSearchAdapter.cs - converts vector results into TextSearchResults
- Services/MafAgent.cs - lightweight agent that calls Azure OpenAI chat and uses retrieved context
- Program.cs - console application and orchestration
- appsettings.json - Azure OpenAI configuration

NuGet packages
- Azure.AI.OpenAI
- Azure.Identity

Azure OpenAI prerequisites
- An Azure OpenAI resource with a chat deployment and an embedding deployment.
- Set the endpoint URL in appsettings.json (for example: https://myresource.openai.azure.com)
- Set deployment names in appsettings.json for ChatDeployment and EmbeddingDeployment.

Authentication
- The code prefers DefaultAzureCredential (managed identity, Visual Studio/Azure CLI auth).
- If DefaultAzureCredential is not available, set the environment variable AZURE_OPENAI_API_KEY with your API key. The code will fall back to using the key.

How to run
1. Fill in appsettings.json with your Azure OpenAI Endpoint and deployment names.
2. (Optional) Set AZURE_OPENAI_API_KEY environment variable if not using DefaultAzureCredential.
3. Build and run the console app. The document will be indexed on startup.
4. Enter questions at the prompt. Type 'exit' to quit.

Notes about Microsoft Agent Framework (MAF)
- This POC includes a minimal, educational adapter (MafTextSearchAdapter) and a small agent (MafAgent) to show how retrieved context is passed to a model for grounded answers.
- In production, replace these components with the official Microsoft Agent Framework TextSearchProvider and Agent APIs. The adapter here is intentionally simple and illustrates the concept.

How indexing works
- The document is loaded from Data/document.txt.
- The DocumentChunker splits the content into overlapping character chunks (default 1000 chars with 150 char overlap).
- Each chunk is sent to Azure OpenAI embedding model and the vector is stored in memory along with the chunk text.

How query retrieval works
- The user question is embedded using the same embedding model.
- Cosine similarity between the question vector and each stored chunk embedding is computed.
- Top 3 matching chunks are returned with similarity scores.
- These chunks are provided to the agent which sends them as context to the Azure OpenAI Chat model.

Limitations and next steps
- This is a learning POC: it uses an in-memory vector store and simple chunking. For larger datasets use a persistent vector DB.
- The MAF components here are illustrative. Swap in the real Microsoft Agent Framework implementations when available.
