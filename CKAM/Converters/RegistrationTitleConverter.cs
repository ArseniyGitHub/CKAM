using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CKAM.Converters
{
    internal class RegistrationTitleConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(value is bool isreg) return isreg ? "Регистрация" : "Вход";
            else return "Регистрация";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
