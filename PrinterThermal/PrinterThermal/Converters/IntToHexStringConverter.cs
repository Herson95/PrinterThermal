namespace PrinterThermal.Converters
{
    using System;
    using System.Globalization;
    using Xamarin.Forms;

    public class IntToHexStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var values = value.ToString();
            var splits = values.Split(' ');
            return $"USB:{int.Parse(splits[0]).ToString("X2")}:{int.Parse(splits[1]).ToString("X2")}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
