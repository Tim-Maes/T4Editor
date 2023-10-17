using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using T4Editor.Common;
using static T4Editor.Common.Constants;

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

            IClassificationType type = null;

            MatchCollection matches = Constants.Regexen.VSOfficialPattern.Matches(document);

            foreach (Match match in matches)
            {
                if (match.Groups["directive"].Success)
                {
                    type = _classificationTypeRegistry.GetClassificationType(Constants.DirectiveBlock);
                    spans.Add(CreateClassificationSpan(snapshot, match.Groups["directive"].Index, match.Groups["directive"].Length, type));
                }

                if (match.Groups["classfeature"].Success)
                {
                    type = _classificationTypeRegistry.GetClassificationType(Constants.ClassFeatureBlock);
                    spans.Add(CreateClassificationSpan(snapshot, match.Groups["classfeature"].Index, match.Groups["classfeature"].Length, type));
                }

                if (match.Groups["expression"].Success)
                {
                    type = _classificationTypeRegistry.GetClassificationType(Constants.ExpressionBlock);
                    spans.Add(CreateClassificationSpan(snapshot, match.Groups["expression"].Index, match.Groups["expression"].Length, type));
                }

                if (match.Groups["statement"].Success)
                {
                    type = _classificationTypeRegistry.GetClassificationType(Constants.ControlBlock);
                    spans.Add(CreateClassificationSpan(snapshot, match.Groups["statement"].Index, match.Groups["statement"].Length, type));
                }

                if (match.Groups["boilerplate"].Success)
                {
                    type = _classificationTypeRegistry.GetClassificationType(Constants.OutputBlock);
                    spans.Add(CreateClassificationSpan(snapshot, match.Groups["boilerplate"].Index, match.Groups["boilerplate"].Length, type));
                }
            }

            return spans;
        }
    }
}