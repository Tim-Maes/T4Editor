using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace T4Editor
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "T4.ClassFeatureBlock")]
    [Name("T4.ClassFeatureBlock")]
    internal sealed class T4ClassFeature : ClassificationFormatDefinition
    {
        public T4ClassFeature()
        {
            this.ForegroundColor = Colors.DodgerBlue;
            this.BackgroundColor = Colors.Transparent;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "T4.StatementBlock")]
    [Name("T4.StatementBlock")]
    internal sealed class T4Statement : ClassificationFormatDefinition
    {
        public T4Statement()
        {
            this.ForegroundColor = Colors.DarkSeaGreen;
            this.BackgroundColor = Colors.Transparent;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "T4.Directive")]
    [Name("T4.Directive")]
    internal sealed class T4Directive : ClassificationFormatDefinition
    {
        public T4Directive()
        {
            this.ForegroundColor = Colors.DarkTurquoise;
            this.BackgroundColor = Colors.Transparent;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "T4.Output")]
    [Name("T4.Output")]
    internal sealed class OutputDirective : ClassificationFormatDefinition
    {
        public OutputDirective()
        {
            this.ForegroundColor = Colors.White;
            this.BackgroundColor = Colors.Transparent;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "T4.Injected")]
    [Name("T4.Injected")]
    internal sealed class InjectedDirective : ClassificationFormatDefinition
    {
        public InjectedDirective()
        {
            this.ForegroundColor = Colors.Yellow;
            this.BackgroundColor = Colors.Transparent;
        }
    }
}
