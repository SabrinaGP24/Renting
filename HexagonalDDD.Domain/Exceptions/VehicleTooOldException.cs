using System;
using System.Globalization;

namespace HexagonalDDD.Domain.Exceptions
{
    public class VehicleTooOldException : DomainException
    {
        public string PlateNumber { get; private set; }

        public DateTime ManufactureDate { get; private set; }

        public int MaxAgeInYears { get; private set; }

        public VehicleTooOldException(string plateNumber, DateTime manufactureDate, int maxAgeInYears)
            : base(string.Format(
                CultureInfo.CurrentCulture,
                "El vehiculo con matricula '{0}' fue fabricado el {1:d} y supera la antiguedad maxima de {2} años permitida en la flota.",
                plateNumber,
                manufactureDate,
                maxAgeInYears))
        {
            PlateNumber = plateNumber;
            ManufactureDate = manufactureDate;
            MaxAgeInYears = maxAgeInYears;
        }
    }
}
