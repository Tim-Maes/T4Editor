using System.Collections.Generic;

namespace T4Editor.Projection
{
    /// <summary>
    /// The result of generating a C# document from T4 source.
    /// Contains the generated code and bidirectional span mappings.
    /// </summary>
    internal class GeneratedDocument
    {
        /// <summary>The complete generated C# source code</summary>
        public string CSharpCode { get; set; }

        /// <summary>Ordered list of span mappings from T4 → generated C#</summary>
        public List<SpanMapping> Mappings { get; set; } = new List<SpanMapping>();

        /// <summary>
        /// Maps a position in the T4 source to the corresponding position in generated C#.
        /// Returns null if the position is not within a mapped C# region.
        /// </summary>
        public int? MapToGenerated(int t4Position)
        {
            foreach (var mapping in Mappings)
            {
                if (t4Position >= mapping.T4Start && t4Position < mapping.T4End)
                {
                    int offsetInSpan = t4Position - mapping.T4Start;
                    if (offsetInSpan < mapping.GeneratedLength)
                    {
                        return mapping.GeneratedStart + offsetInSpan;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Maps a position in the generated C# back to the T4 source.
        /// Returns null if the position is not within a mapped region.
        /// </summary>
        public int? MapFromGenerated(int generatedPosition)
        {
            foreach (var mapping in Mappings)
            {
                if (generatedPosition >= mapping.GeneratedStart && generatedPosition < mapping.GeneratedEnd)
                {
                    int offsetInSpan = generatedPosition - mapping.GeneratedStart;
                    if (offsetInSpan < mapping.T4Length)
                    {
                        return mapping.T4Start + offsetInSpan;
                    }
                }
            }
            return null;
        }
    }
}
