using System;

namespace MafRagPoc.Models
{
    /// <summary>
    /// Represents a single chunk of the original document.
    /// Holds text content, a source identifier and the embedding vector produced by the model.
    /// The embedding is nullable until generated.
    /// </summary>
    public class DocumentChunk
    {
        /// <summary>Unique identifier for this chunk.</summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Source document name (for example, document.txt).</summary>
        public string SourceName { get; set; } = string.Empty;

        /// <summary>The chunk's textual content.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Embedding vector for the chunk; null until generated.</summary>
        public IReadOnlyList<float>? Embedding { get; set; }
    }
}
