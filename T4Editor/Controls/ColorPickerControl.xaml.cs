using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace T4Editor.Controls
{
    public partial class ColorPickerControl : UserControl
    {
        public ColorPickerControl()
        {
            InitializeComponent();
            StatemenBlockColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Settings.Default.StatementBlockColor);
            FeatureBlockColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Settings.Default.FeatureBlockColor);
            DirectiveColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Settings.Default.DirectiveColor);
            OutputColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Settings.Default.OutputColor);
            InjectedColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Settings.Default.InjectedColor);
        }

        private void StatemenBlockColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Settings.Default.StatementBlockColor = StatemenBlockColorPicker.SelectedColor.ToString();
            Settings.Default.Save();
        }

        private void FeatureBlockColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Settings.Default.FeatureBlockColor = FeatureBlockColorPicker.SelectedColor.ToString();
            Settings.Default.Save();
        }

        private void DirectiveColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Settings.Default.DirectiveColor = DirectiveColorPicker.SelectedColor.ToString();
            Settings.Default.Save();
        }

        private void OutputColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Settings.Default.OutputColor = OutputColorPicker.SelectedColor.ToString();
            Settings.Default.Save();
        }

        private void InjectedColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Settings.Default.InjectedColor = InjectedColorPicker.SelectedColor.ToString();
            Settings.Default.Save();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var myWindow = Window.GetWindow(this);
            myWindow.Close();
        }
    }
}
