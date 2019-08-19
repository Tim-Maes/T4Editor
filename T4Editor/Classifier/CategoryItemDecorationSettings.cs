using System.Windows.Media;

namespace T4Editor.Classifier
{
    class CategoryItemDecorationSettings 
    {
        private Color _foregroundColor;
        private Color _backgroundColor;

        public string DisplayName { get; set; }

        public Color ForegroundColor
        {
            get { return _foregroundColor; }
            set
            {
                if (value != _foregroundColor)
                {
                    _foregroundColor = value;
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
                }
            }
        }
    }
}
