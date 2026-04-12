using System.Globalization;
using System.Windows.Data;

namespace Maximus.Converters;

public sealed class ProgressBarWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double width && !double.IsNaN(width) && !double.IsInfinity(width))
        {
            return Math.Max(0d, width);
        }

        return 0d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
