using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using T4Editor.Common;

namespace T4Editor.Performance
{
    /// <summary>
    /// Performance optimization helper for T4 classification operations
    /// </summary>
    internal class T4ClassificationCache
    {
        private readonly Dictionary<string, IClassificationType> _typeCache;
        private ITextSnapshot _lastSnapshot;
        private List<ClassificationSpan> _cachedSpans;

        public T4ClassificationCache()
        {
            _typeCache = new Dictionary<string, IClassificationType>();
        }

        /// <summary>
        /// Initialize and cache classification types to avoid repeated registry lookups
        /// </summary>
        public void InitializeCache(IClassificationTypeRegistryService registry)
        {
            _typeCache[Constants.Tag] = registry.GetClassificationType(Constants.Tag);
            _typeCache[Constants.DirectiveBlock] = registry.GetClassificationType(Constants.DirectiveBlock);
            _typeCache[Constants.ControlBlock] = registry.GetClassificationType(Constants.ControlBlock);
            _typeCache[Constants.ClassFeatureBlock] = registry.GetClassificationType(Constants.ClassFeatureBlock);
            _typeCache[Constants.ExpressionBlock] = registry.GetClassificationType(Constants.ExpressionBlock);
            _typeCache[Constants.OutputBlock] = registry.GetClassificationType(Constants.OutputBlock);
        }

        /// <summary>
        /// Get cached classification type
        /// </summary>
        public IClassificationType GetCachedType(string typeName)
        {
            return _typeCache.TryGetValue(typeName, out var type) ? type : null;
        }

        /// <summary>
        /// Try to get cached spans for a snapshot
        /// </summary>
        public bool TryGetCachedSpans(ITextSnapshot snapshot, out List<ClassificationSpan> spans)
        {
            spans = null;
            if (_lastSnapshot == snapshot && _cachedSpans != null)
            {
                spans = _cachedSpans;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cache spans for a snapshot
        /// </summary>
        public void CacheSpans(ITextSnapshot snapshot, List<ClassificationSpan> spans)
        {
            _lastSnapshot = snapshot;
            _cachedSpans = new List<ClassificationSpan>(spans);
        }

        /// <summary>
        /// Filter spans to only return those that intersect with the requested range
        /// </summary>
        public List<ClassificationSpan> FilterSpansForRange(List<ClassificationSpan> allSpans, SnapshotSpan requestedSpan)
        {
            if (allSpans == null) return new List<ClassificationSpan>();

            var result = new List<ClassificationSpan>();
            foreach (var span in allSpans)
            {
                if (span.Span.IntersectsWith(requestedSpan))
                {
                    result.Add(span);
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Simple T4 parser for large files when regex performance becomes an issue
    /// </summary>
    internal class SimpleT4Parser
    {
        /// <summary>
        /// Parse T4 content using simple string operations instead of regex
        /// </summary>
        public static List<ClassificationSpan> ParseWithStringOperations(string text, ITextSnapshot snapshot, 
            int offset, T4ClassificationCache cache)
        {
            var spans = new List<ClassificationSpan>();
            int position = 0;

            while (position < text.Length)
            {
                int blockStart = text.IndexOf("<#", position);
                if (blockStart == -1)
                {
                    // Rest is output text
                    if (position < text.Length)
                    {
                        AddSpan(spans, snapshot, offset + position, text.Length - position, 
                            cache.GetCachedType(Constants.OutputBlock));
                    }
                    break;
                }

                // Output text before block
                if (blockStart > position)
                {
                    AddSpan(spans, snapshot, offset + position, blockStart - position, 
                        cache.GetCachedType(Constants.OutputBlock));
                }

                // Find block end
                int blockEnd = text.IndexOf("#>", blockStart + 2);
                if (blockEnd == -1) break;

                // Classify the block
                ClassifyBlock(spans, text, snapshot, offset, blockStart, blockEnd, cache);

                position = blockEnd + 2;
            }

            return spans;
        }

        private static void ClassifyBlock(List<ClassificationSpan> spans, string text, ITextSnapshot snapshot, 
            int offset, int blockStart, int blockEnd, T4ClassificationCache cache)
        {
            string contentType;
            int tagLength;

            // Determine block type based on opening characters
            if (blockStart + 2 < text.Length && text[blockStart + 2] == '@')
            {
                contentType = Constants.DirectiveBlock;
                tagLength = 3;
            }
            else if (blockStart + 2 < text.Length && text[blockStart + 2] == '+')
            {
                contentType = Constants.ClassFeatureBlock;
                tagLength = 3;
            }
            else if (blockStart + 2 < text.Length && text[blockStart + 2] == '=')
            {
                contentType = Constants.ExpressionBlock;
                tagLength = 3;
            }
            else
            {
                contentType = Constants.ControlBlock;
                tagLength = 2;
            }

            var tagType = cache.GetCachedType(Constants.Tag);
            var blockContentType = cache.GetCachedType(contentType);

            // Opening tag
            AddSpan(spans, snapshot, offset + blockStart, tagLength, tagType);

            // Content
            int contentLength = blockEnd - blockStart - tagLength;
            if (contentLength > 0)
            {
                AddSpan(spans, snapshot, offset + blockStart + tagLength, contentLength, blockContentType);
            }

            // Closing tag
            AddSpan(spans, snapshot, offset + blockEnd, 2, tagType);
        }

        private static void AddSpan(List<ClassificationSpan> spans, ITextSnapshot snapshot, int start, int length, 
            IClassificationType type)
        {
            if (length > 0 && type != null)
            {
                spans.Add(new ClassificationSpan(new SnapshotSpan(new SnapshotPoint(snapshot, start), length), type));
            }
        }
    }
}