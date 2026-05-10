using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace WorkFocusManager.Converters
{
    public class BlackListBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Brushes.Transparent;

            if(value is bool isBlock)
            {
                if(isBlock)
                    return Brushes.Pink;
                else
                    return Brushes.Transparent;
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
            => Binding.DoNothing;
    }
}
