using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;
using T4Editor.Common;

namespace T4Editor
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ControlBlock)]
    [Name(Constants.ControlBlock)]
    internal sealed class T4ClassFeature : ClassificationFormatDefinition
    {
        public T4ClassFeature()
        {
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString(Settings.Default.ControlBlockColor);
            this.BackgroundColor = Colors.Transparent;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ClassFeatureBlock)]
    [Name(Constants.ClassFeatureBlock)]
    internal sealed class T4Statement : ClassificationFormatDefinition
    {
        public T4Statement()
        {
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString(Settings.Default.ClassFeatureBlockColor);
            this.BackgroundColor = Colors.Transparent;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.DirectiveBlock)]
    [Name(Constants.DirectiveBlock)]
    internal sealed class T4Directive : ClassificationFormatDefinition
    {
        public T4Directive()
        {
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString(Settings.Default.DirectiveColor);
            this.BackgroundColor = Colors.Transparent;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.OutputBlock)]
    [Name(Constants.OutputBlock)]
    internal sealed class OutputDirective : ClassificationFormatDefinition
    {
        public OutputDirective()
        {
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString(Settings.Default.OutputColor);
            this.BackgroundColor = Colors.Transparent;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.ExpressionBlock)]
    [Name(Constants.ExpressionBlock)]
    internal sealed class InjectedDirective : ClassificationFormatDefinition
    {
        public InjectedDirective()
        {
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString(Settings.Default.InjectedColor);
            this.BackgroundColor = Colors.Transparent;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.Tag)]
    [Name(Constants.Tag)]
    internal sealed class TagDirective : ClassificationFormatDefinition
    {
        public TagDirective()
        {
            this.ForegroundColor = (Color)ColorConverter.ConvertFromString(Settings.Default.TagColor);
            this.BackgroundColor = (Color)ColorConverter.ConvertFromString(Settings.Default.TagBackground);
        }
    }
}
