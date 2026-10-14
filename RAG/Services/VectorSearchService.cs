using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MafRagPoc.Models;

namespace MafRagPoc.Services
{
    /// <summary>
    /// In-memory vector store for storing document chunks and their embeddings.
    /// Provides AddAsync to store chunks and SearchAsync to find top-k similar chunks for a query.
    /// </summary>
    public class VectorSearchService
    {
        // Simple list backing vector store (POC only).
        private readonly List<DocumentChunk> _store = new();

        // Embedding service used to generate query embeddings on search.
        private readonly EmbeddingService _embeddingService;

        public VectorSearchService(EmbeddingService embeddingService)
        {
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        }

        /// <summary>
        /// Adds chunks to the in-memory store. Chunks must already have embeddings.
        /// </summary>
        public Task AddAsync(IEnumerable<DocumentChunk> chunks)
        {
            if (chunks == null) throw new ArgumentNullException(nameof(chunks));
            _store.AddRange(chunks);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Searches the store for the topK most similar chunks to the provided question.
        /// </summary>
        public async Task<IReadOnlyList<SearchResult>> SearchAsync(string question, int topK = 3, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(question)) return new List<SearchResult>();
            var qEmbedding = await _embeddingService.GenerateEmbeddingAsync(question, cancellationToken).ConfigureAwait(false);
            var results = new List<SearchResult>();
            foreach (var chunk in _store)
            {
                if (chunk.Embedding == null) continue;
                float sim = CosineSimilarity(qEmbedding, chunk.Embedding);
                results.Add(new SearchResult(chunk, sim));
            }

            return results.OrderByDescending(r => r.Similarity).Take(topK).ToList();
        }

        // Cosine similarity between two vectors: (A . B) / (|A| * |B|)
        private static float CosineSimilarity(IReadOnlyList<float> a, IReadOnlyList<float> b)
        {
            if (a == null || b == null) return 0f;
            if (a.Count != b.Count) return 0f;
            double dot = 0, magA = 0, magB = 0;
            for (int i = 0; i < a.Count; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            if (magA == 0 || magB == 0) return 0f;
            return (float)(dot / (Math.Sqrt(magA) * Math.Sqrt(magB)));
        }
    }

    public sealed record SearchResult(DocumentChunk Chunk, double Similarity);

}
