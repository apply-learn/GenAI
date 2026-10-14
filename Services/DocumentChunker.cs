using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MafRagPoc.Services
{
    /// <summary>
    /// Splits long documents into overlapping chunks suitable for embedding and search.
    /// </summary>
    public class DocumentChunker
    {
        public int ChunkSize { get; }

        public int Overlap { get; }

        public DocumentChunker(int chunkSize = 1000, int overlap = 150)
        {
            if (chunkSize <= 0) throw new ArgumentException("chunkSize must be > 0", nameof(chunkSize));
            if (overlap < 0) throw new ArgumentException("overlap must be >= 0", nameof(overlap));
            ChunkSize = chunkSize;
            Overlap = overlap;
        }

        /// <summary>
        /// Produces a list of text chunks from the input text using the configured chunk size
        /// and overlap between adjacent chunks.
        /// </summary>
        public Task<List<string>> ChunkAsync(string text)
        {
            var chunks = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return Task.FromResult(chunks);

            int pos = 0;
            int length = text.Length;
            while (pos < length)
            {
                int take = Math.Min(ChunkSize, length - pos);
                string chunk = text.Substring(pos, take);
                chunks.Add(chunk);
                if (pos + take >= length) break;
                pos += take - Overlap;
                if (pos < 0) pos = 0;
            }

            return Task.FromResult(chunks);
        }
    }
}
