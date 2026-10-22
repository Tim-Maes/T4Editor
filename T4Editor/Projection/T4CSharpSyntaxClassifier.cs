using Microsoft.CodeAnalysis.Text;
using RoslynClassification = Microsoft.CodeAnalysis.Classification;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace T4Editor.Projection
{
    /// <summary>
    /// Provides C# syntax highlighting inside T4 code blocks by querying Roslyn's
    /// classification service and mapping spans back to the T4 buffer.
    /// </summary>
    [Export(typeof(IClassifierProvider))]
    [ContentType("T4")]
    [Name("T4 C# Syntax Classifier")]
    [Order(After = "T4ClassifierProvider")]
    internal class T4CSharpSyntaxClassifierProvider : IClassifierProvider
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(
                typeof(T4CSharpSyntaxClassifier),
                () => new T4CSharpSyntaxClassifier(textBuffer, ClassificationRegistry));
        }
    }

    /// <summary>
    /// Roslyn classification runs in the background whenever the projection is regenerated.
    /// <see cref="GetClassificationSpans"/> only reads the latest results, translating them
    /// to the requested snapshot while newer results are still being computed.
    /// </summary>
    internal class T4CSharpSyntaxClassifier : IClassifier
    {
        private readonly ITextBuffer _buffer;
        private readonly IClassificationTypeRegistryService _registry;
        private readonly object _resultsLock = new object();
        private T4ProjectionBufferManager _manager;
        private volatile ClassificationResults _results;

        public T4CSharpSyntaxClassifier(
            ITextBuffer buffer,
            IClassificationTypeRegistryService registry)
        {
            _buffer = buffer;
            _registry = registry;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            EnsureSubscribed();

            var result = new List<ClassificationSpan>();
            var results = _results;
            if (results == null)
                return result;

            if (results.Snapshot == span.Snapshot)
            {
                foreach (var classified in results.Spans)
                {
                    if (classified.Span.IntersectsWith(span))
                        result.Add(classified);
                }
                return result;
            }

            // Results lag behind while the projection is regenerated after an edit:
            // translate them so colors follow the text until fresh results arrive
            var requested = span.TranslateTo(results.Snapshot, SpanTrackingMode.EdgeInclusive);
            foreach (var classified in results.Spans)
            {
                if (!classified.Span.IntersectsWith(requested))
                    continue;

                var translated = classified.Span.TranslateTo(span.Snapshot, SpanTrackingMode.EdgeExclusive);
                if (translated.Length > 0 && translated.IntersectsWith(span))
                    result.Add(new ClassificationSpan(translated, classified.ClassificationType));
            }
            return result;
        }

        /// <summary>
        /// Attaches to the buffer's projection manager, which is created by a view listener
        /// and replaced if all views close and the buffer is opened again. UI thread only.
        /// </summary>
        private void EnsureSubscribed()
        {
            _buffer.Properties.TryGetProperty(typeof(T4ProjectionBufferManager), out T4ProjectionBufferManager manager);
            if (manager == _manager)
                return;

            if (_manager != null)
                _manager.StateChanged -= OnStateChanged;

            _manager = manager;

            if (_manager != null)
            {
                _manager.StateChanged += OnStateChanged;
                OnStateChanged(_manager, EventArgs.Empty);
            }
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            var state = ((T4ProjectionBufferManager)sender).CurrentState;
            if (state == null || _results?.State == state)
                return;

            _ = ThreadHelper.JoinableTaskFactory.RunAsync(() => ClassifyAsync(state));
        }

        private async Task ClassifyAsync(T4ProjectionState state)
        {
            await TaskScheduler.Default;

            List<ClassificationSpan> spans;
            try
            {
                spans = await GetClassificationSpansAsync(state);
            }
            catch (Exception)
            {
                return;
            }

            lock (_resultsLock)
            {
                // A newer state may have finished first; never go backwards
                var current = _results;
                if (current != null && current.Snapshot.Version.VersionNumber >= state.Snapshot.Version.VersionNumber)
                    return;

                _results = new ClassificationResults(state, spans);
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var snapshot = _buffer.CurrentSnapshot;
            ClassificationChanged?.Invoke(this, new ClassificationChangedEventArgs(
                new SnapshotSpan(snapshot, 0, snapshot.Length)));
        }

        private async Task<List<ClassificationSpan>> GetClassificationSpansAsync(T4ProjectionState state)
        {
            var text = await state.Document.GetTextAsync();
            var classifiedSpans = await RoslynClassification.Classifier.GetClassifiedSpansAsync(
                state.Document, TextSpan.FromBounds(0, text.Length), CancellationToken.None);

            // Map Roslyn classifications back to T4 positions
            var snapshot = state.Snapshot;
            var result = new List<ClassificationSpan>();
            foreach (var classified in classifiedSpans)
            {
                var classificationType = MapRoslynClassification(classified.ClassificationType);
                if (classificationType == null)
                    continue;

                // Map start and end from generated C# back to T4
                int? t4Start = state.Generated.MapFromGenerated(classified.TextSpan.Start);
                int? t4End = state.Generated.MapFromGenerated(classified.TextSpan.End);

                if (!t4Start.HasValue || !t4End.HasValue)
                    continue;

                int start = t4Start.Value;
                int length = t4End.Value - start;

                if (start < 0 || length <= 0 || start + length > snapshot.Length)
                    continue;

                result.Add(new ClassificationSpan(
                    new SnapshotSpan(snapshot, start, length),
                    classificationType));
            }

            return result;
        }

        private IClassificationType MapRoslynClassification(string roslynType)
        {
            // Map Roslyn classification types to VS editor classification types
            string vsType;
            switch (roslynType)
            {
                case RoslynClassification.ClassificationTypeNames.Keyword:
                case RoslynClassification.ClassificationTypeNames.ControlKeyword:
                    vsType = "keyword";
                    break;
                case RoslynClassification.ClassificationTypeNames.ClassName:
                case RoslynClassification.ClassificationTypeNames.RecordClassName:
                    vsType = "class name";
                    break;
                case RoslynClassification.ClassificationTypeNames.StructName:
                case RoslynClassification.ClassificationTypeNames.RecordStructName:
                    vsType = "struct name";
                    break;
                case RoslynClassification.ClassificationTypeNames.InterfaceName:
                    vsType = "interface name";
                    break;
                case RoslynClassification.ClassificationTypeNames.EnumName:
                    vsType = "enum name";
                    break;
                case RoslynClassification.ClassificationTypeNames.DelegateName:
                    vsType = "delegate name";
                    break;
                case RoslynClassification.ClassificationTypeNames.TypeParameterName:
                    vsType = "type parameter name";
                    break;
                case RoslynClassification.ClassificationTypeNames.StringLiteral:
                case RoslynClassification.ClassificationTypeNames.VerbatimStringLiteral:
                    vsType = "string";
                    break;
                case RoslynClassification.ClassificationTypeNames.NumericLiteral:
                    vsType = "number";
                    break;
                case RoslynClassification.ClassificationTypeNames.Comment:
                    vsType = "comment";
                    break;
                case RoslynClassification.ClassificationTypeNames.MethodName:
                case RoslynClassification.ClassificationTypeNames.ExtensionMethodName:
                    vsType = "method name";
                    break;
                case RoslynClassification.ClassificationTypeNames.PropertyName:
                    vsType = "property name";
                    break;
                case RoslynClassification.ClassificationTypeNames.FieldName:
                case RoslynClassification.ClassificationTypeNames.ConstantName:
                case RoslynClassification.ClassificationTypeNames.EnumMemberName:
                    vsType = "field name";
                    break;
                case RoslynClassification.ClassificationTypeNames.LocalName:
                case RoslynClassification.ClassificationTypeNames.ParameterName:
                    vsType = "local name";
                    break;
                case RoslynClassification.ClassificationTypeNames.NamespaceName:
                    vsType = "namespace name";
                    break;
                case RoslynClassification.ClassificationTypeNames.Operator:
                case RoslynClassification.ClassificationTypeNames.OperatorOverloaded:
                    vsType = "operator";
                    break;
                case RoslynClassification.ClassificationTypeNames.Punctuation:
                    vsType = "punctuation";
                    break;
                case RoslynClassification.ClassificationTypeNames.PreprocessorKeyword:
                    vsType = "preprocessor keyword";
                    break;
                default:
                    return null;
            }

            return _registry.GetClassificationType(vsType);
        }

        private sealed class ClassificationResults
        {
            public ClassificationResults(T4ProjectionState state, List<ClassificationSpan> spans)
            {
                State = state;
                Spans = spans;
            }

            public T4ProjectionState State { get; }
            public ITextSnapshot Snapshot => State.Snapshot;
            public List<ClassificationSpan> Spans { get; }
        }
    }
}
