using System;
using System.Globalization;
using System.Windows.Data;

namespace WPF_Hexagonal_DDD.Views.Converters
{
    /// <summary>
    /// Traduce el estado tecnico del DTO (Available/Rented) al texto que ve el usuario.
    /// La traduccion es responsabilidad de la presentacion, no de la capa de aplicacion.
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    public class VehicleStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;

            if (string.Equals(status, "Available", StringComparison.OrdinalIgnoreCase))
            {
                return "Disponible";
            }

            if (string.Equals(status, "Rented", StringComparison.OrdinalIgnoreCase))
            {
                return "Alquilado";
            }

            return status;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
