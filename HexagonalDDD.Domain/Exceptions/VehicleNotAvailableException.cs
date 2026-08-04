using System;
using System.Globalization;

namespace HexagonalDDD.Domain.Exceptions
{
    public class VehicleNotAvailableException : DomainException
    {
        public Guid VehicleId { get; private set; }

        public string PlateNumber { get; private set; }

        public VehicleNotAvailableException(Guid vehicleId, string plateNumber)
            : base(string.Format(CultureInfo.CurrentCulture, "El vehiculo '{0}' no esta disponible: ya se encuentra alquilado.", plateNumber))
        {
            VehicleId = vehicleId;
            PlateNumber = plateNumber;
        }
    }
}
