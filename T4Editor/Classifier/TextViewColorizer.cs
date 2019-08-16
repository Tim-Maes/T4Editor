using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using T4Editor.Extensions;

namespace T4Editor.Classifier
{
    internal class TextViewColorizer
    {
        // Disable "Field is never assigned to..." compiler's warning. Justification: the field is assigned by MEF.
#pragma warning disable 649

        [Import]
        private IClassificationFormatMapService _classificationFormatMapService;

        [Import]
        private IClassificationTypeRegistryService _registryService;

#pragma warning restore 649

        private readonly ITextView _textView;
        private IClassificationFormatMap _classificationFormatMap;
        private IEnumerable<IClassificationType> _T4Types;

        internal TextViewColorizer(ITextView textView)
        {
            _textView = textView;
            this.SatisfyImportsOnce();
        }

        internal void UpdateColors(IEnumerable<CategoryItemDecorationSettings> changedItems)
        {
            if (_classificationFormatMapService == null || _registryService == null) SatisfyImports();
            if (_classificationFormatMap == null) _classificationFormatMap = _classificationFormatMapService.GetClassificationFormatMap(_textView);
            if (_T4Types == null) _T4Types = _classificationFormatMap.CurrentPriorityOrder.Where(x => x?.Classification.Contains("T4") == true).ToList();

            try
            {
                if (_classificationFormatMap.IsInBatchUpdate) return;

                _classificationFormatMap.BeginBatchUpdate();

                foreach (CategoryItemDecorationSettings changedItem in changedItems)
                {
                    string classificationKey = changedItem.DisplayName;
                    IClassificationType classificationType = _T4Types.FirstOrDefault(x => x.Classification.Contains(classificationKey));
                    if (classificationType == null) continue;

                    TextFormattingRunProperties textProperties = CreateTextProperties(changedItem);
                    _classificationFormatMap.SetExplicitTextProperties(classificationType, textProperties);
                }
            }
            catch (Exception)
            {
                //TO-DO: Log exception
            }
            finally
            {
                _classificationFormatMap.EndBatchUpdate();
            }
        }

        private void SatisfyImports()
        {
            if (Common.Instances.ServiceProvider != null)
            {
                Common.Instances.ServiceProvider.SatisfyImportsOnce(this);
            }
        }

        private TextFormattingRunProperties CreateTextProperties(CategoryItemDecorationSettings colorSetting)
        {
            TextFormattingRunProperties textFormatting = TextFormattingRunProperties.CreateTextFormattingRunProperties();
            textFormatting = textFormatting.SetBackground(colorSetting.BackgroundColor);
            textFormatting = textFormatting.SetForeground(colorSetting.ForegroundColor);

            return textFormatting;
        }
    }
}
