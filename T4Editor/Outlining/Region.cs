namespace T4Editor.Outlining
{
    class PartialRegion
    {
        public int StartLine { get; set; }
        public int StartOffset { get; set; }
        public int Level { get; set; }
        public PartialRegion PartialParent { get; set; }
    }

    class Region : PartialRegion
    {
        public int EndLine { get; set; }
    }

    class Block
    {
        public const string ControlBlockStartHide = "<#";
        public const string ControlBlockEndHide = "#>";
        public const string ControlBlockEllipsis = "<#..#>";
        public const string ControlBlockHoverText = "hidden block";
    }
}
