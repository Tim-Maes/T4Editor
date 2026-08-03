namespace T4Editor.Projection
{
    /// <summary>
    /// Maps a span in the T4 source to a span in the generated C# document.
    /// Used for bidirectional position translation.
    /// </summary>
    internal struct SpanMapping
    {
        /// <summary>Start position in the .tt source file</summary>
        public int T4Start { get; set; }

        /// <summary>Length of the span in the .tt source file</summary>
        public int T4Length { get; set; }

        /// <summary>Start position in the generated .cs document</summary>
        public int GeneratedStart { get; set; }

        /// <summary>Length of the span in the generated .cs document</summary>
        public int GeneratedLength { get; set; }

        /// <summary>The type of T4 block this mapping belongs to</summary>
        public T4MappingBlockType BlockType { get; set; }

        public int T4End => T4Start + T4Length;
        public int GeneratedEnd => GeneratedStart + GeneratedLength;
    }

    internal enum T4MappingBlockType
    {
        UsingDirective,
        ControlBlock,
        ExpressionBlock,
        ClassFeatureBlock
    }
}
