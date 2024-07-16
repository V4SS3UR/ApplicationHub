using System;
using System.Globalization;
using System.Windows.Data;

namespace ApplicationHub.Core.Converters
{
    public class GreaterThanConverter : IValueConverter
    {
        public int Threshold { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue > Threshold;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
