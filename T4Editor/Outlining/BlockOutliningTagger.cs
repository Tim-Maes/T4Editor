using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using T4Editor.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace T4Editor.Outlining
{
    internal sealed class BlockOutliningTagger : ITagger<IOutliningRegionTag>
    {
        private readonly ITextBuffer buffer;
        private ITextSnapshot snapshot;
        private List<Region> regions;

        // Performance optimization: Track if we need to reparse
        private bool _needsReparse = true;

        // Performance optimization: Cache for frequently accessed strings
        private static readonly string BlockStart = Constants.BlockStartHide;
        private static readonly string BlockEnd = Constants.BlockEndHide;

        public BlockOutliningTagger(ITextBuffer buffer)
        {
            this.buffer = buffer;
            this.snapshot = buffer.CurrentSnapshot;
            this.regions = new List<Region>();
            this.buffer.Changed += BufferChanged;

            // Defer initial parsing for better startup performance
            this._needsReparse = true;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
                yield break;

            // Performance optimization: Only reparse if needed
            if (_needsReparse)
            {
                ReParse();
                _needsReparse = false;
            }

            List<Region> currentRegions = this.regions;
            ITextSnapshot currentSnapshot = this.snapshot;

            if (currentRegions.Count == 0)
                yield break;

            SnapshotSpan entire = new SnapshotSpan(spans[0].Start, spans[spans.Count - 1].End)
                .TranslateTo(currentSnapshot, SpanTrackingMode.EdgeExclusive);

            int startLineNumber = entire.Start.GetContainingLine().LineNumber;
            int endLineNumber = entire.End.GetContainingLine().LineNumber;

            // Performance optimization: Use binary search for large region collections
            foreach (var region in GetRegionsInRange(currentRegions, startLineNumber, endLineNumber))
            {
                var startLine = currentSnapshot.GetLineFromLineNumber(region.StartLine);
                var endLine = currentSnapshot.GetLineFromLineNumber(region.EndLine);

                yield return new TagSpan<IOutliningRegionTag>(
                    new SnapshotSpan(startLine.Start + region.StartOffset, endLine.End),
                    new OutliningRegionTag(false, false, Constants.BlockEllipsis, Constants.BlockHoverText));
            }
        }

        /// <summary>
        /// Performance optimization: Get regions that intersect with the specified line range
        /// </summary>
        private IEnumerable<Region> GetRegionsInRange(List<Region> regions, int startLine, int endLine)
        {
            // For small collections, simple iteration is faster than binary search
            if (regions.Count < 50)
            {
                foreach (var region in regions)
                {
                    if (region.StartLine <= endLine && region.EndLine >= startLine)
                        yield return region;
                }
            }
            else
            {
                // For large collections, use more sophisticated filtering
                int startIndex = FindFirstRegionAtOrAfter(regions, startLine);

                for (int i = startIndex; i < regions.Count && regions[i].StartLine <= endLine; i++)
                {
                    if (regions[i].EndLine >= startLine)
                        yield return regions[i];
                }
            }
        }

        /// <summary>
        /// Binary search to find the first region at or after the specified line
        /// </summary>
        private int FindFirstRegionAtOrAfter(List<Region> regions, int targetLine)
        {
            int left = 0;
            int right = regions.Count - 1;
            int result = regions.Count;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;

                if (regions[mid].EndLine >= targetLine)
                {
                    result = mid;
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            return result;
        }

        void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
            if (e.After != buffer.CurrentSnapshot)
                return;

            // Mark for reparsing instead of immediately reparsing
            _needsReparse = true;
        }

        void ReParse()
        {
            ITextSnapshot newSnapshot = buffer.CurrentSnapshot;
            List<Region> newRegions = new List<Region>();

            // Performance optimization: Use faster parsing for T4 blocks
            ParseT4Blocks(newSnapshot, newRegions);

            // Performance optimization: Only fire change events if regions actually changed
            if (RegionsChanged(this.regions, newRegions))
            {
                var oldSnapshot = this.snapshot;
                this.snapshot = newSnapshot;
                this.regions = newRegions;

                FireTagsChangedEvent(oldSnapshot, newSnapshot);
            }
            else
            {
                this.snapshot = newSnapshot;
            }
        }

        /// <summary>
        /// Optimized T4 block parsing using IndexOf instead of line-by-line iteration
        /// </summary>
        private void ParseT4Blocks(ITextSnapshot snapshot, List<Region> regions)
        {
            string text = snapshot.GetText();
            var blockStack = new Stack<PartialRegion>();

            int position = 0;

            while (position < text.Length)
            {
                int blockStartPos = text.IndexOf(BlockStart, position);
                if (blockStartPos == -1)
                    break;

                // Find the end of this block
                int blockEndPos = text.IndexOf(BlockEnd, blockStartPos + BlockStart.Length);
                if (blockEndPos == -1)
                    break;

                // Convert positions to line numbers and offsets
                var startLine = snapshot.GetLineFromPosition(blockStartPos);
                var endLine = snapshot.GetLineFromPosition(blockEndPos + BlockEnd.Length - 1);

                int startOffset = blockStartPos - startLine.Start.Position;

                // Only create region if it spans multiple lines or is significant in size
                if (endLine.LineNumber > startLine.LineNumber ||
                    (blockEndPos - blockStartPos) > 50)
                {
                    regions.Add(new Region
                    {
                        Level = blockStack.Count + 1,
                        StartLine = startLine.LineNumber,
                        StartOffset = startOffset,
                        EndLine = endLine.LineNumber
                    });
                }

                position = blockEndPos + BlockEnd.Length;
            }
        }

        /// <summary>
        /// Check if regions have actually changed to avoid unnecessary updates
        /// </summary>
        private bool RegionsChanged(List<Region> oldRegions, List<Region> newRegions)
        {
            if (oldRegions.Count != newRegions.Count)
                return true;

            for (int i = 0; i < oldRegions.Count; i++)
            {
                var oldRegion = oldRegions[i];
                var newRegion = newRegions[i];

                if (oldRegion.StartLine != newRegion.StartLine ||
                    oldRegion.EndLine != newRegion.EndLine ||
                    oldRegion.StartOffset != newRegion.StartOffset)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Fire tags changed event for the affected range
        /// </summary>
        private void FireTagsChangedEvent(ITextSnapshot oldSnapshot, ITextSnapshot newSnapshot)
        {
            var oldSpans = this.regions.Select(r => AsSnapshotSpan(r, oldSnapshot)).ToList();
            var newSpans = this.regions.Select(r => AsSnapshotSpan(r, newSnapshot)).ToList();

            if (oldSpans.Any() || newSpans.Any())
            {
                int changeStart = 0;
                int changeEnd = newSnapshot.Length;

                if (oldSpans.Any())
                {
                    changeStart = Math.Min(changeStart, oldSpans.Min(s => s.Start.Position));
                    changeEnd = Math.Max(changeEnd, oldSpans.Max(s => s.End.Position));
                }

                if (newSpans.Any())
                {
                    changeStart = Math.Min(changeStart, newSpans.Min(s => s.Start.Position));
                    changeEnd = Math.Max(changeEnd, newSpans.Max(s => s.End.Position));
                }

                this.TagsChanged?.Invoke(this,
                    new SnapshotSpanEventArgs(new SnapshotSpan(newSnapshot,
                        Span.FromBounds(changeStart, changeEnd))));
            }
        }

        static bool TryGetLevel(string text, int startIndex, out int level)
        {
            level = -1;
            if (text.Length > startIndex + 3)
            {
                if (int.TryParse(text.Substring(startIndex + 1), out level))
                    return true;
            }

            return false;
        }

        static SnapshotSpan AsSnapshotSpan(Region region, ITextSnapshot snapshot)
        {
            var startLine = snapshot.GetLineFromLineNumber(region.StartLine);
            var endLine = (region.StartLine == region.EndLine) ? startLine
                 : snapshot.GetLineFromLineNumber(region.EndLine);
            return new SnapshotSpan(startLine.Start + region.StartOffset, endLine.End);
        }
    }
}
