using System;
using System.Globalization;

namespace HexagonalDDD.Domain.Exceptions
{
    public class VehicleNotRentedException : DomainException
    {
        public Guid VehicleId { get; private set; }

        public string PlateNumber { get; private set; }

        public VehicleNotRentedException(Guid vehicleId, string plateNumber)
            : base(string.Format(CultureInfo.CurrentCulture, "El vehiculo '{0}' no esta alquilado, por lo que no puede devolverse.", plateNumber))
        {
            VehicleId = vehicleId;
            PlateNumber = plateNumber;
        }
    }
}
