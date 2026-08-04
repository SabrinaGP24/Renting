using System;
using System.Globalization;

namespace HexagonalDDD.Domain.Exceptions
{
    public class ActiveRentalNotFoundException : DomainException
    {
        public Guid VehicleId { get; private set; }

        public ActiveRentalNotFoundException(Guid vehicleId)
            : base(string.Format(CultureInfo.CurrentCulture, "No existe un alquiler activo para el vehiculo '{0}'.", vehicleId))
        {
            VehicleId = vehicleId;
        }
    }
}
