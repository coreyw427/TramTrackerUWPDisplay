using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace TramTrackerUWPDisplay
{
    public class DateTimeDifferenceConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTime dateTime = (DateTime)value;
            Double minutes = dateTime.Subtract(DateTime.Now).TotalMinutes;
            int roundedMinutes = (int)Math.Round(minutes, 0);
            return roundedMinutes.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
