using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using T4Editor.Common;
using T4Editor.Performance;
using static T4Editor.Common.Constants;

namespace T4Editor
{
    internal class T4Classifier : IClassifier
    {
        private readonly ITextBuffer _buffer;
        IClassificationTypeRegistryService _classificationTypeRegistry;

        // Performance optimization: Use caching helper
        private readonly T4ClassificationCache _cache;
        
        // New token-based parser
        private readonly T4TokenBasedParser _tokenParser;

        internal T4Classifier(ITextBuffer buffer, IClassificationTypeRegistryService registry)
        {
            this._buffer = buffer;
            this._classificationTypeRegistry = registry;
            this._cache = new T4ClassificationCache();
            this._cache.InitializeCache(registry);
            this._tokenParser = new T4TokenBasedParser(_cache);
        }

#pragma warning disable 67

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        protected virtual void OnClassificationChanged(ClassificationChangedEventArgs e)
        {
            ClassificationChanged?.Invoke(this, e);
        }

        public ClassificationSpan CreateClassificationSpan(ITextSnapshot snapshot, int index, int length, IClassificationType type)
        {
            return new ClassificationSpan(new SnapshotSpan(new SnapshotPoint(snapshot, index), length), type);
        }

#pragma warning restore 67

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            ITextSnapshot snapshot = span.Snapshot;

            List<ClassificationSpan> spans = new List<ClassificationSpan>();

            if (snapshot.Length == 0)
                return spans;

            // Performance optimization: Try to use cached results
            if (_cache.TryGetCachedSpans(snapshot, out var cachedSpans))
            {
                return _cache.FilterSpansForRange(cachedSpans, span);
            }

            // Performance optimization: For large files, use different strategy
            if (snapshot.Length > LARGE_FILE_THRESHOLD && span.Length < snapshot.Length)
            {
                return ProcessLargeFileSpan(span);
            }

            // Process the entire document and cache results
            var document = snapshot.GetText();

            try
            {
                // Use the new token-based parser as the primary method
                spans = _tokenParser.Parse(document, snapshot);

                // Cache the results for future use
                _cache.CacheSpans(snapshot, spans);
            }
            catch (Exception ex)
            {
                // Fall back to regex parsing if token parser fails for any reason
                try
                {
                    ProcessDocumentWithRegex(spans, document, snapshot);
                    _cache.CacheSpans(snapshot, spans);
                }
                catch (RegexMatchTimeoutException)
                {
                    // Final fallback to simple parsing
                    spans.Clear();
                    spans.AddRange(SimpleT4Parser.ParseWithStringOperations(document, snapshot, 0, _cache));
                }
            }

            return spans;
        }

        /// <summary>
        /// Process a span of a large file with extended boundaries to capture complete blocks
        /// </summary>
        private List<ClassificationSpan> ProcessLargeFileSpan(SnapshotSpan span)
        {
            var snapshot = span.Snapshot;
            var document = snapshot.GetText();

            try
            {
                // Use token-based parser for range parsing
                return _tokenParser.ParseRange(document, snapshot, span.Start, span.Length);
            }
            catch (Exception)
            {
                // Fallback to the extended regex approach
                int start = Math.Max(0, span.Start - 1000);
                int end = Math.Min(snapshot.Length, span.End + 1000);

                var extendedText = snapshot.GetText(start, end - start);
                List<ClassificationSpan> allSpans;

                try
                {
                    allSpans = new List<ClassificationSpan>();
                    ProcessDocumentWithRegex(allSpans, extendedText, snapshot, start);
                }
                catch (RegexMatchTimeoutException)
                {
                    // Use simple parser for problematic content
                    allSpans = SimpleT4Parser.ParseWithStringOperations(extendedText, snapshot, start, _cache);
                }

                return _cache.FilterSpansForRange(allSpans, span);
            }
        }

        /// <summary>
        /// Legacy regex-based processing - kept as fallback
        /// </summary>
        private void ProcessDocumentWithRegex(List<ClassificationSpan> spans, string document, ITextSnapshot snapshot, int offset = 0)
        {
            IClassificationType type = null;

            MatchCollection directiveMatches = Regexen.Directive.Matches(document);
            foreach (Match match in directiveMatches)
            {
                if (match.Success)
                {
                    var openingTagMatch = match.Groups["openingtag"];
                    var codeMatch = match.Groups["code"];
                    var opclosingTagMatch = match.Groups["closingtag"];

                    type = _cache.GetCachedType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, offset + openingTagMatch.Index, openingTagMatch.Value.Length, type));

                    type = _cache.GetCachedType(Constants.DirectiveBlock);
                    spans.Add(CreateClassificationSpan(snapshot, offset + codeMatch.Index, codeMatch.Value.Length, type));

                    type = _cache.GetCachedType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, offset + opclosingTagMatch.Index, opclosingTagMatch.Value.Length, type));
                }
            }

            MatchCollection controlBlockMatches = Regexen.ControlBlock.Matches(document);
            foreach (Match match in controlBlockMatches)
            {
                if (match.Success)
                {
                    var openingTagMatch = match.Groups["openingtag"];
                    var codeMatch = match.Groups["code"];
                    var opclosingTagMatch = match.Groups["closingtag"];

                    type = _cache.GetCachedType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, offset + openingTagMatch.Index, openingTagMatch.Value.Length, type));

                    type = _cache.GetCachedType(Constants.ControlBlock);
                    spans.Add(CreateClassificationSpan(snapshot, offset + codeMatch.Index, codeMatch.Value.Length, type));

                    type = _cache.GetCachedType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, offset + opclosingTagMatch.Index, opclosingTagMatch.Value.Length, type));
                }
            }

            MatchCollection classFeatureBlockMatches = Regexen.ClassFeatureBlock.Matches(document);
            foreach (Match match in classFeatureBlockMatches)
            {
                if (match.Success)
                {
                    var openingTagMatch = match.Groups["openingtag"];
                    var codeMatch = match.Groups["code"];
                    var opclosingTagMatch = match.Groups["closingtag"];

                    type = _cache.GetCachedType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, offset + openingTagMatch.Index, openingTagMatch.Value.Length, type));

                    type = _cache.GetCachedType(Constants.ClassFeatureBlock);
                    spans.Add(CreateClassificationSpan(snapshot, offset + codeMatch.Index, codeMatch.Value.Length, type));

                    type = _cache.GetCachedType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, offset + opclosingTagMatch.Index, opclosingTagMatch.Value.Length, type));
                }
            }

            MatchCollection expressionBlockMatches = Regexen.ExpressionBlock.Matches(document);
            foreach (Match match in expressionBlockMatches)
            {
                if (match.Success)
                {
                    var openingTagMatch = match.Groups["openingtag"];
                    var codeMatch = match.Groups["code"];
                    var opclosingTagMatch = match.Groups["closingtag"];

                    type = _cache.GetCachedType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, offset + openingTagMatch.Index, openingTagMatch.Value.Length, type));

                    type = _cache.GetCachedType(Constants.ExpressionBlock);
                    spans.Add(CreateClassificationSpan(snapshot, offset + codeMatch.Index, codeMatch.Value.Length, type));

                    type = _cache.GetCachedType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, offset + opclosingTagMatch.Index, opclosingTagMatch.Value.Length, type));
                }
            }

            MatchCollection outputMatches = Regexen.OutputBlock.Matches(document);
            foreach (Match match in outputMatches)
            {
                if (match.Success)
                {
                    type = _cache.GetCachedType(Constants.OutputBlock);
                    spans.Add(CreateClassificationSpan(snapshot, offset + match.Index, match.Value.Length, type));
                }
            }
        }
    }
}