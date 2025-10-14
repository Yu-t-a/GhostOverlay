using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GhostOverlay.Models;
using Color = System.Windows.Media.Color;

namespace GhostOverlay.Helpers;

public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EndpointStatus status)
        {
            return status switch
            {
                EndpointStatus.Up => new SolidColorBrush(Color.FromRgb(40, 167, 69)),      // Green
                EndpointStatus.Slow => new SolidColorBrush(Color.FromRgb(255, 193, 7)),    // Yellow
                EndpointStatus.Down => new SolidColorBrush(Color.FromRgb(220, 53, 69)),    // Red
                EndpointStatus.Unknown => new SolidColorBrush(Color.FromRgb(108, 117, 125)), // Gray
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
