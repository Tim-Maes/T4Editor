using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace T4Editor.Classifier
{
    [Export]
    internal class TextViewColorizersManager
    {
        private ICollection<TextViewColorizer> Colorizers { get; }

        internal TextViewColorizersManager()
        {
            Colorizers = new List<TextViewColorizer>();
        }

        internal void AddColorizer(TextViewColorizer colorizer)
        {
            Colorizers.Add(colorizer);
        }

        internal IEnumerable<TextViewColorizer> GetColorizers()
        {
            return Colorizers;
        }
    }
}
