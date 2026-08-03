using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RoslynClassification = Microsoft.CodeAnalysis.Classification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    internal class T4CSharpSyntaxClassifier : IClassifier
    {
        private readonly ITextBuffer _buffer;
        private readonly IClassificationTypeRegistryService _registry;
        private ITextSnapshot _lastSnapshot;
        private List<ClassificationSpan> _cachedSpans = new List<ClassificationSpan>();

        public T4CSharpSyntaxClassifier(
            ITextBuffer buffer,
            IClassificationTypeRegistryService registry)
        {
            _buffer = buffer;
            _registry = registry;
            _buffer.Changed += OnBufferChanged;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        private void OnBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // Invalidate cache; reclassify on next request
            _lastSnapshot = null;
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var snapshot = span.Snapshot;

            // Use cached results if snapshot hasn't changed
            if (_lastSnapshot == snapshot && _cachedSpans.Count > 0)
            {
                return FilterSpans(_cachedSpans, span);
            }

            if (!_buffer.Properties.TryGetProperty(typeof(T4ProjectionBufferManager), out T4ProjectionBufferManager manager))
                return new List<ClassificationSpan>();

            var document = manager.WorkspaceManager?.Document;
            var generatedDoc = manager.CurrentDocument;
            if (document == null || generatedDoc == null)
                return new List<ClassificationSpan>();

            // Get Roslyn classifications (run synchronously on thread pool to avoid deadlock)
            List<RoslynClassification.ClassifiedSpan> classifiedSpans;
            try
            {
                classifiedSpans = Task.Run(() => GetClassificationsAsync(document)).Result;
            }
            catch (Exception)
            {
                return new List<ClassificationSpan>();
            }

            if (classifiedSpans == null)
                return new List<ClassificationSpan>();

            // Map Roslyn classifications back to T4 positions
            var result = new List<ClassificationSpan>();
            foreach (var classified in classifiedSpans)
            {
                var classificationType = MapRoslynClassification(classified.ClassificationType);
                if (classificationType == null)
                    continue;

                // Map start and end from generated C# back to T4
                int? t4Start = generatedDoc.MapFromGenerated(classified.TextSpan.Start);
                int? t4End = generatedDoc.MapFromGenerated(classified.TextSpan.End);

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

            _cachedSpans = result;
            _lastSnapshot = snapshot;

            return FilterSpans(result, span);
        }

        private async Task<List<RoslynClassification.ClassifiedSpan>> GetClassificationsAsync(Document document)
        {
            var text = await document.GetTextAsync();
            var spans = await Microsoft.CodeAnalysis.Classification.Classifier.GetClassifiedSpansAsync(
                document, TextSpan.FromBounds(0, text.Length), CancellationToken.None);
            return spans.ToList();
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

        private List<ClassificationSpan> FilterSpans(List<ClassificationSpan> spans, SnapshotSpan range)
        {
            var result = new List<ClassificationSpan>();
            foreach (var span in spans)
            {
                if (span.Span.IntersectsWith(range))
                    result.Add(span);
            }
            return result;
        }
    }
}
