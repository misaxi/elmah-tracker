using System.Windows;

namespace ElmahTracker
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();

            textBoxRssUrl.Text = ConfigManager.Config.RssUrl;

            sliderInterval.Value = ConfigManager.Config.Interval;
        }

        private void OnClickCloseButton(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnClickOkButton(object sender, RoutedEventArgs e)
        {
            ConfigManager.SaveConfig(textBoxRssUrl.Text, (int)sliderInterval.Value);

            Close();
        }
    }
}