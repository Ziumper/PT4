﻿using PT4.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PT4.DialogWindow
{
    public class DirectionBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.Equals(ConvertEnum(parameter)))
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == false) return null;
            return ConvertEnum(parameter);
        }

        private Direction ConvertEnum(object parameter)
        {
            int intValue = Int32.Parse((string)parameter);
            Direction enumValue = (Direction)intValue;

            return enumValue;
        }
    }
}
