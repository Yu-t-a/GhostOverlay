using System.Diagnostics;
using System.IO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace GhostOverlay.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OpenConfig_Click(object sender, RoutedEventArgs e)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appData, "GhostOverlay");

        if (Directory.Exists(appFolder))
        {
            Process.Start("explorer.exe", appFolder);
        }
        else
        {
            MessageBox.Show(
                "Configuration folder not found. It will be created on first run.",
                "Information",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
