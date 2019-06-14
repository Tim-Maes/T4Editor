using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace T4Editor.Classifier
{
    internal class TextViewColorizer
    {
#pragma warning disable 649

        [Import]
        private IClassificationFormatMapService _classificationFormatMapService;

        [Import]
        private IClassificationTypeRegistryService _registryService;

#pragma warning restore 649

        private readonly ITextView _textView;
        private IClassificationFormatMap _classificationFormatMap;
        private IEnumerable<IClassificationType> _sharpLizerTypes;

        internal TextViewColorizer(ITextView textView)
        {
            _textView = textView;
        }

        internal void UpdateColors()
        {
            try
            {
                if (_classificationFormatMap.IsInBatchUpdate) return;

                _classificationFormatMap.BeginBatchUpdate();

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
    }
}
