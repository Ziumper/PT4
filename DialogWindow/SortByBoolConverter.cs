using PT4.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PT4.DialogWindow
{
    public class SortByBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            SortBy enumValue = ConvertEnum(parameter);
       
            if (value.Equals(enumValue))
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == false) return null;
            return ConvertEnum(parameter);
        }

        
        private SortBy ConvertEnum(object parameter)
        {
            int intValue = Int32.Parse((string)parameter);
            SortBy enumValue = (SortBy)intValue;

            return enumValue;
        }
    }
}
