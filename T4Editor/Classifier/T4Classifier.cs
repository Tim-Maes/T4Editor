using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using T4Editor.Common;

namespace T4Editor
{
    internal class T4Classifier : IClassifier
    {
        private readonly ITextBuffer _buffer;
        IClassificationTypeRegistryService _classificationTypeRegistry;

        internal T4Classifier(ITextBuffer buffer, IClassificationTypeRegistryService registry)
        {
            this._buffer = buffer;
            this._classificationTypeRegistry = registry;
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

            var document = snapshot.GetText();

            MatchCollection directiveMatches = Regex.Matches(document, Constants.DirectiveRegex);
            MatchCollection controlBlockMatches = Regex.Matches(document, Constants.ControlBlockRegex);
            MatchCollection classFeatureBlockMatches = Regex.Matches(document, Constants.ClassFeatureBlockRegex);
            MatchCollection expressionBlockMatches = Regex.Matches(document, Constants.ExpressionBlockRegex);
            MatchCollection outputMatches = Regex.Matches(document, Constants.OutputBlockRegex);

            IClassificationType type = null;

            foreach (Match match in directiveMatches)
            {
                if (match.Success)
                {
                    var openingTagMatch = match.Groups["openingtag"];
                    var codeMatch = match.Groups["code"];
                    var opclosingTagMatch = match.Groups["closingtag"];

                    type = _classificationTypeRegistry.GetClassificationType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, openingTagMatch.Index, openingTagMatch.Value.Length, type));

                    type = _classificationTypeRegistry.GetClassificationType(Constants.DirectiveBlock);
                    spans.Add(CreateClassificationSpan(snapshot, codeMatch.Index, codeMatch.Value.Length, type));

                    type = _classificationTypeRegistry.GetClassificationType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, opclosingTagMatch.Index, opclosingTagMatch.Value.Length, type));
                }
            }

            foreach (Match match in controlBlockMatches)
            {
                if (match.Success)
                {
                    var openingTagMatch = match.Groups["openingtag"];
                    var codeMatch = match.Groups["code"];
                    var opclosingTagMatch = match.Groups["closingtag"];

                    type = _classificationTypeRegistry.GetClassificationType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, openingTagMatch.Index, openingTagMatch.Value.Length, type));

                    type = _classificationTypeRegistry.GetClassificationType(Constants.ControlBlock);
                    spans.Add(CreateClassificationSpan(snapshot, codeMatch.Index, codeMatch.Value.Length, type));

                    type = _classificationTypeRegistry.GetClassificationType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, opclosingTagMatch.Index, opclosingTagMatch.Value.Length, type));
                }
            }

            foreach (Match match in classFeatureBlockMatches)
            {
                if (match.Success)
                {
                    var openingTagMatch = match.Groups["openingtag"];
                    var codeMatch = match.Groups["code"];
                    var opclosingTagMatch = match.Groups["closingtag"];

                    type = _classificationTypeRegistry.GetClassificationType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, openingTagMatch.Index, openingTagMatch.Value.Length, type));

                    type = _classificationTypeRegistry.GetClassificationType(Constants.ClassFeatureBlock);
                    spans.Add(CreateClassificationSpan(snapshot, codeMatch.Index, codeMatch.Value.Length, type));

                    type = _classificationTypeRegistry.GetClassificationType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, opclosingTagMatch.Index, opclosingTagMatch.Value.Length, type));
                }
            }

            foreach (Match match in expressionBlockMatches)
            {
                if (match.Success)
                {
                    var openingTagMatch = match.Groups["openingtag"];
                    var codeMatch = match.Groups["code"];
                    var opclosingTagMatch = match.Groups["closingtag"];

                    type = _classificationTypeRegistry.GetClassificationType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, openingTagMatch.Index, openingTagMatch.Value.Length, type));

                    type = _classificationTypeRegistry.GetClassificationType(Constants.ExpressionBlock);
                    spans.Add(CreateClassificationSpan(snapshot, codeMatch.Index, codeMatch.Value.Length, type));

                    type = _classificationTypeRegistry.GetClassificationType(Constants.Tag);
                    spans.Add(CreateClassificationSpan(snapshot, opclosingTagMatch.Index, opclosingTagMatch.Value.Length, type));
                }
            }

            foreach (Match match in outputMatches)
            {
                if (match.Success)
                {
                    type = _classificationTypeRegistry.GetClassificationType(Constants.OutputBlock);
                    spans.Add(CreateClassificationSpan(snapshot, match.Index, match.Value.Length, type));
                }
            }

            return spans;
        }
    }
}

