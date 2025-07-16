using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using T4Editor.Common;

namespace T4Editor.Performance
{
    /// <summary>
    /// Improved token-based T4 parser for better performance and extensibility
    /// This replaces the complex RegEx patterns with a linear parsing approach
    /// </summary>
    internal class T4TokenBasedParser
    {
        private readonly T4ClassificationCache _cache;

        public T4TokenBasedParser(T4ClassificationCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Parse T4 content using token-based approach
        /// </summary>
        public List<ClassificationSpan> Parse(string text, ITextSnapshot snapshot, int offset = 0)
        {
            var spans = new List<ClassificationSpan>();
            var parser = new T4DocumentParser(_cache);
            
            return parser.ParseDocument(text, snapshot, offset);
        }

        /// <summary>
        /// Parse only a specific range for large file optimization
        /// </summary>
        public List<ClassificationSpan> ParseRange(string text, ITextSnapshot snapshot, int start, int length, int offset = 0)
        {
            int expandedStart = Math.Max(0, start - 200);
            int expandedEnd = Math.Min(text.Length, start + length + 200);
            
            string rangeText = text.Substring(expandedStart, expandedEnd - expandedStart);
            var allSpans = Parse(rangeText, snapshot, offset + expandedStart);

            // Filter to only return spans within the requested range
            var result = new List<ClassificationSpan>();
            int rangeStart = offset + start;
            int rangeEnd = offset + start + length;

            foreach (var span in allSpans)
            {
                if (span.Span.IntersectsWith(new SnapshotSpan(snapshot, rangeStart, rangeEnd - rangeStart)))
                {
                    result.Add(span);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// T4 block types for classification
    /// </summary>
    internal enum T4BlockType
    {
        None,
        Control,        // <# ... #>
        ClassFeature,   // <#+ ... #>
        Expression,     // <#= ... #>
        Directive       // <#@ ... #>
    }

    /// <summary>
    /// Document parser that processes T4 content linearly
    /// </summary>
    internal class T4DocumentParser
    {
        private readonly T4ClassificationCache _cache;

        public T4DocumentParser(T4ClassificationCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public List<ClassificationSpan> ParseDocument(string text, ITextSnapshot snapshot, int offset = 0)
        {
            var spans = new List<ClassificationSpan>();
            int position = 0;

            while (position < text.Length)
            {
                int blockStart = text.IndexOf("<#", position);
                
                if (blockStart == -1)
                {
                    if (position < text.Length)
                    {
                        AddOutputSpan(spans, snapshot, offset + position, text.Length - position);
                    }
                    break;
                }

                if (blockStart > position)
                {
                    AddOutputSpan(spans, snapshot, offset + position, blockStart - position);
                }

                var blockEnd = ParseT4Block(text, blockStart, spans, snapshot, offset);
                if (blockEnd > blockStart)
                {
                    position = blockEnd;
                }
                else
                {
                    position = blockStart + 2;
                }
            }

            return spans;
        }

        private int ParseT4Block(string text, int blockStart, List<ClassificationSpan> spans, ITextSnapshot snapshot, int offset)
        {
            if (blockStart + 1 >= text.Length)
            {
                AddOutputSpan(spans, snapshot, offset + blockStart, text.Length - blockStart);
                return text.Length;
            }

            var blockType = DetermineBlockType(text, blockStart);
            int openTagLength = GetOpenTagLength(blockType);

            if (blockStart + openTagLength > text.Length)
            {
                AddOutputSpan(spans, snapshot, offset + blockStart, text.Length - blockStart);
                return text.Length;
            }

            int closeTagStart = FindClosingTag(text, blockStart + openTagLength);
            if (closeTagStart == -1)
            {
                AddOutputSpan(spans, snapshot, offset + blockStart, text.Length - blockStart);
                return text.Length;
            }

            var tagType = _cache.GetCachedType(Constants.Tag);
            AddSpan(spans, snapshot, offset + blockStart, openTagLength, tagType);

            int contentStart = blockStart + openTagLength;
            int contentLength = closeTagStart - contentStart;
            if (contentLength > 0)
            {
                var contentType = GetContentClassificationType(blockType);
                if (contentType != null)
                {
                    AddSpan(spans, snapshot, offset + contentStart, contentLength, contentType);
                }
            }

            AddSpan(spans, snapshot, offset + closeTagStart, 2, tagType);

            return closeTagStart + 2;
        }

        private T4BlockType DetermineBlockType(string text, int blockStart)
        {
            if (blockStart + 2 >= text.Length)
                return T4BlockType.Control; // Default

            char thirdChar = text[blockStart + 2];
            switch (thirdChar)
            {
                case '@': return T4BlockType.Directive;
                case '+': return T4BlockType.ClassFeature;
                case '=': return T4BlockType.Expression;
                default: return T4BlockType.Control;
            }
        }

        private int GetOpenTagLength(T4BlockType blockType)
        {
            return blockType == T4BlockType.Control ? 2 : 3;
        }

        private int FindClosingTag(string text, int searchStart)
        {
            int position = searchStart;
            
            while (position < text.Length - 1)
            {
                int closePos = text.IndexOf("#>", position);
                if (closePos == -1)
                    return -1;

                // Check if this is really a closing tag (not inside a string, etc.)
                // For now, we'll accept any #> as a closing tag
                // This could be enhanced later to handle strings and comments
                return closePos;
            }

            return -1;
        }

        private IClassificationType GetContentClassificationType(T4BlockType blockType)
        {
            switch (blockType)
            {
                case T4BlockType.Control:
                    return _cache.GetCachedType(Constants.ControlBlock);
                case T4BlockType.ClassFeature:
                    return _cache.GetCachedType(Constants.ClassFeatureBlock);
                case T4BlockType.Expression:
                    return _cache.GetCachedType(Constants.ExpressionBlock);
                case T4BlockType.Directive:
                    return _cache.GetCachedType(Constants.DirectiveBlock);
                default:
                    return null;
            }
        }

        private void AddOutputSpan(List<ClassificationSpan> spans, ITextSnapshot snapshot, int start, int length)
        {
            if (length > 0)
            {
                var outputType = _cache.GetCachedType(Constants.OutputBlock);
                AddSpan(spans, snapshot, start, length, outputType);
            }
        }

        private void AddSpan(List<ClassificationSpan> spans, ITextSnapshot snapshot, int start, int length, IClassificationType type)
        {
            if (length > 0 && type != null && start >= 0 && start + length <= snapshot.Length)
            {
                spans.Add(new ClassificationSpan(new SnapshotSpan(new SnapshotPoint(snapshot, start), length), type));
            }
        }
    }

    /// <summary>
    /// Enhanced error detection for T4 blocks
    /// This can be extended in the future to provide better error reporting
    /// </summary>
    internal class T4ErrorDetector
    {
        public static List<T4Error> DetectErrors(string text)
        {
            var errors = new List<T4Error>();
            int position = 0;

            while (position < text.Length)
            {
                int openTag = text.IndexOf("<#", position);
                if (openTag == -1)
                    break;

                int closeTag = text.IndexOf("#>", openTag + 2);
                if (closeTag == -1)
                {
                    // Unclosed block
                    errors.Add(new T4Error
                    {
                        Type = T4ErrorType.UnclosedBlock,
                        Position = openTag,
                        Length = text.Length - openTag,
                        Message = "T4 block is not closed"
                    });
                    break;
                }

                position = closeTag + 2;
            }

            return errors;
        }
    }

    /// <summary>
    /// Represents a T4 parsing error
    /// </summary>
    internal struct T4Error
    {
        public T4ErrorType Type { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Types of T4 parsing errors
    /// </summary>
    internal enum T4ErrorType
    {
        UnclosedBlock,
        InvalidSyntax,
        UnexpectedCharacter
    }
} 