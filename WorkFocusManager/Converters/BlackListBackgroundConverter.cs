using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WorkFocusManager.Converters
{
    public class BlackListBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is true ? System.Windows.Media.Brushes.Pink : System.Windows.Media.Brushes.Transparent;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => System.Windows.Data.Binding.DoNothing;
    }
}
