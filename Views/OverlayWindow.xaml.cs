using System;
using System.Windows;
using System.Windows.Input;
using GhostOverlay.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace GhostOverlay.Views;

public partial class OverlayWindow : Window
{
    public OverlayWindow()
    {
        InitializeComponent();
        LoadWindowPosition();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            try
            {
                this.DragMove();
                SaveWindowPosition();
            }
            catch
            {
                // DragMove can throw if window is not being dragged properly
            }
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        this.Hide();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsWindow = new SettingsWindow
            {
                Owner = this
            };
            settingsWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error opening Settings window:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                "Settings Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void AddMonitor_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var addMonitorWindow = new AddMonitorWindow
            {
                Owner = this
            };

            if (addMonitorWindow.ShowDialog() == true && addMonitorWindow.NewEndpoint != null)
            {
                // Get ViewModel and add the endpoint
                if (DataContext is OverlayViewModel vm)
                {
                    vm.AddEndpoint(addMonitorWindow.NewEndpoint);

                    MessageBox.Show(
                        $"Monitor '{addMonitorWindow.NewEndpoint.Name}' has been added successfully!",
                        "Monitor Added",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error adding monitor:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                "Add Monitor Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is OverlayViewModel vm)
        {
            vm.RefreshCommand.Execute(null);
        }
    }

    private void LoadWindowPosition()
    {
        // For now, center on screen
        // Later we can save/load from settings
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void SaveWindowPosition()
    {
        // TODO: Save to settings
    }
}
