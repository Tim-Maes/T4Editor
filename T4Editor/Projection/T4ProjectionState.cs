using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace T4Editor.Projection
{
    /// <summary>
    /// An immutable view of the C# projection of a T4 buffer: the T4 snapshot it was
    /// generated from, the generated code with its span mappings, and the Roslyn document
    /// holding exactly that code. Consumers must use all three from the same state so
    /// positions, mappings and Roslyn results always agree.
    /// </summary>
    internal sealed class T4ProjectionState
    {
        public T4ProjectionState(ITextSnapshot snapshot, GeneratedDocument generated, Document document)
        {
            Snapshot = snapshot;
            Generated = generated;
            Document = document;
        }

        /// <summary>The T4 snapshot the projection was generated from</summary>
        public ITextSnapshot Snapshot { get; }

        /// <summary>The generated C# code and its span mappings</summary>
        public GeneratedDocument Generated { get; }

        /// <summary>The Roslyn document containing <see cref="GeneratedDocument.CSharpCode"/></summary>
        public Document Document { get; }
    }
}
