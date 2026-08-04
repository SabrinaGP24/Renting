using System;
using HexagonalDDD.Domain.Exceptions;

namespace HexagonalDDD.Domain.Aggregates.Vehicle
{
    public class VehicleAggregate
    {
        public const int MaxAgeInYears = 5;

        private VehicleAggregate(
            Guid id,
            string plateNumber,
            string brand,
            string model,
            DateTime manufactureDate,
            VehicleStatus status)
        {
            Id = id;
            PlateNumber = plateNumber;
            Brand = brand;
            Model = model;
            ManufactureDate = manufactureDate;
            Status = status;
        }

        public Guid Id { get; private set; }

        public string PlateNumber { get; private set; }

        public string Brand { get; private set; }

        public string Model { get; private set; }

        public DateTime ManufactureDate { get; private set; }

        public VehicleStatus Status { get; private set; }

        /// <summary>
        /// Alta de un vehiculo en la flota. <paramref name="referenceDate"/> se recibe como parametro
        /// (y no se lee de DateTime.Now) para que la regla de antiguedad sea testeable con fecha fija.
        /// </summary>
        public static VehicleAggregate Create(
            Guid id,
            string plateNumber,
            string brand,
            string model,
            DateTime manufactureDate,
            DateTime referenceDate)
        {
            if (id == Guid.Empty)
            {
                throw new DomainValidationException("El identificador del vehiculo es obligatorio.");
            }

            string normalizedPlate = Normalize(plateNumber);
            if (normalizedPlate.Length == 0)
            {
                throw new DomainValidationException("La matricula es obligatoria.");
            }

            string normalizedBrand = Normalize(brand);
            if (normalizedBrand.Length == 0)
            {
                throw new DomainValidationException("La marca es obligatoria.");
            }

            string normalizedModel = Normalize(model);
            if (normalizedModel.Length == 0)
            {
                throw new DomainValidationException("El modelo es obligatorio.");
            }

            DateTime manufactureDay = manufactureDate.Date;
            DateTime referenceDay = referenceDate.Date;

            if (manufactureDay > referenceDay)
            {
                throw new DomainValidationException("La fecha de fabricacion no puede ser futura.");
            }

            if (manufactureDay < referenceDay.AddYears(-MaxAgeInYears))
            {
                throw new VehicleTooOldException(normalizedPlate, manufactureDay, MaxAgeInYears);
            }

            return new VehicleAggregate(
                id,
                normalizedPlate.ToUpperInvariant(),
                normalizedBrand,
                normalizedModel,
                manufactureDay,
                VehicleStatus.Available);
        }

        /// <summary>
        /// Reconstruye el agregado desde persistencia sin re-aplicar las invariantes de alta:
        /// un vehiculo dado de alta hace años sigue siendo valido aunque hoy ya no cumpliria la regla.
        /// </summary>
        public static VehicleAggregate Rehydrate(
            Guid id,
            string plateNumber,
            string brand,
            string model,
            DateTime manufactureDate,
            VehicleStatus status)
        {
            return new VehicleAggregate(id, plateNumber, brand, model, manufactureDate, status);
        }

        public void Rent()
        {
            if (Status != VehicleStatus.Available)
            {
                throw new VehicleNotAvailableException(Id, PlateNumber);
            }

            Status = VehicleStatus.Rented;
        }

        public void Return()
        {
            if (Status != VehicleStatus.Rented)
            {
                throw new VehicleNotRentedException(Id, PlateNumber);
            }

            Status = VehicleStatus.Available;
        }

        private static string Normalize(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }
}
