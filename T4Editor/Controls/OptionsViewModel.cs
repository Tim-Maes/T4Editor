using System.ComponentModel.Composition;
using T4Editor.Classifier;

namespace T4Editor.Controls
{
    internal class OptionsViewModel
    {
        [Import]
        private TextViewColorizerManager _textViewsManager;

        public OptionsViewModel()
        {
        }

        public void UpdateColors()
        {
            if (_textViewsManager == null) return;
            
            foreach (TextViewColorizer colorizer in _textViewsManager.GetColorizers())
            {
                colorizer.UpdateColors();
            }
        }
    }
}
