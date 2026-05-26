using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WorkFocusManager.Converters
{
    public class CharacterImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var resourcePath = value?.ToString() switch
            {
                "Dog" => "pack://application:,,,/Resources/DogImage.gif",
                "Bear" => "pack://application:,,,/Resources/BearImage.gif",
                _ => "pack://application:,,,/Resources/CatImage.gif"
            };

            return new BitmapImage(new Uri(resourcePath, UriKind.Absolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
