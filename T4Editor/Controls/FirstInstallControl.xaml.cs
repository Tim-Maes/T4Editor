using System.Windows;
using System.Windows.Controls;

namespace T4Editor.Controls
{
    public partial class FirstInstallControl : UserControl
    {
        public FirstInstallControl()
        {
            InitializeComponent();
        }

        public string SelectedValue => (selectThemeComboBox.SelectedItem as ComboBoxItem)?.Content as string;

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.DialogResult = true;
            window.Close();
        }
    }
}
