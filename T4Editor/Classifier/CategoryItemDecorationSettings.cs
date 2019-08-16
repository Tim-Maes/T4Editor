using System.Windows.Media;
using T4Editor.Classifier.Helpers;

namespace T4Editor.Classifier
{
    class CategoryItemDecorationSettings : NotifiesPropertyChanged
    {
        private Color _foregroundColor;
        private Color _backgroundColor;
        private bool _hasChanges;

        public string DisplayName { get; set; }

        public Color ForegroundColor
        {
            get { return _foregroundColor; }
            set
            {
                if (value != _foregroundColor)
                {
                    _foregroundColor = value;
                    OnPropertyChanged();
                    this.HasChanges = true;
                }
            }
        }

        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                if (value != _backgroundColor)
                {
                    _backgroundColor = value;
                    OnPropertyChanged();
                    this.HasChanges = true;
                }
            }
        }

        public bool HasChanges
        {
            get { return _hasChanges; }
            set
            {
                _hasChanges = value;
                OnPropertyChanged();
            }
        }
    }
}
