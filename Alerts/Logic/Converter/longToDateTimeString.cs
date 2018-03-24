using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Alerts.Logic.Converter
{
    public class longToDateTimeString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string v = "";

            try
            {
                v = (new DateTime(1970, 1, 1)).AddMilliseconds((long)value).ToString("yyyy-MM-dd HH:mm");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("EXCP: " + e.Message + " " + e.StackTrace);
                throw;
            }

            return v;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return DateTime.ParseExact((string)value, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("EXCP: " + e.Message +" " + e.StackTrace);
                throw;
            }


        }
    }
}
