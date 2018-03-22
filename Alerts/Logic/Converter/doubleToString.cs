using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Alerts.Logic.Converter
{
    public class doubleToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value).ToString("0.00", CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return double.Parse((string)value, CultureInfo.InvariantCulture);
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("EXCP: " + e.Message + " " + e.StackTrace);
                throw;
            }

        }
    }
}
