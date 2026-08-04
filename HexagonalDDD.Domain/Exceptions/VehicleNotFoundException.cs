using System;
using System.Globalization;

namespace HexagonalDDD.Domain.Exceptions
{
    public class VehicleNotFoundException : DomainException
    {
        public Guid VehicleId { get; private set; }

        public VehicleNotFoundException(Guid vehicleId)
            : base(string.Format(CultureInfo.CurrentCulture, "No existe ningun vehiculo con identificador '{0}'.", vehicleId))
        {
            VehicleId = vehicleId;
        }
    }
}
