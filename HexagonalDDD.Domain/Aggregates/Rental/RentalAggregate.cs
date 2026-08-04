using System;
using HexagonalDDD.Domain.Aggregates.Customer;
using HexagonalDDD.Domain.Exceptions;

namespace HexagonalDDD.Domain.Aggregates.Rental
{
    public class RentalAggregate
    {
        private RentalAggregate(
            Guid id,
            Guid vehicleId,
            string customerDocumentId,
            DateTime startDate,
            DateTime? endDate,
            RentalStatus status)
        {
            Id = id;
            VehicleId = vehicleId;
            CustomerDocumentId = customerDocumentId;
            StartDate = startDate;
            EndDate = endDate;
            Status = status;
        }

        public Guid Id { get; private set; }

        public Guid VehicleId { get; private set; }

        public string CustomerDocumentId { get; private set; }

        public DateTime StartDate { get; private set; }

        public DateTime? EndDate { get; private set; }

        public RentalStatus Status { get; private set; }

        public static RentalAggregate Start(Guid id, Guid vehicleId, string customerDocumentId, DateTime startDate)
        {
            if (id == Guid.Empty)
            {
                throw new DomainValidationException("El identificador del alquiler es obligatorio.");
            }

            if (vehicleId == Guid.Empty)
            {
                throw new DomainValidationException("El identificador del vehiculo es obligatorio.");
            }

            string normalizedDocument = CustomerAggregate.NormalizeDocumentId(customerDocumentId);
            if (normalizedDocument.Length == 0)
            {
                throw new DomainValidationException("El documento identificativo del cliente es obligatorio.");
            }

            return new RentalAggregate(
                id,
                vehicleId,
                normalizedDocument,
                startDate,
                null,
                RentalStatus.Active);
        }

        public static RentalAggregate Rehydrate(
            Guid id,
            Guid vehicleId,
            string customerDocumentId,
            DateTime startDate,
            DateTime? endDate,
            RentalStatus status)
        {
            return new RentalAggregate(id, vehicleId, customerDocumentId, startDate, endDate, status);
        }

        public void Close(DateTime endDate)
        {
            if (Status != RentalStatus.Active)
            {
                throw new DomainValidationException("El alquiler ya fue devuelto previamente.");
            }

            if (endDate < StartDate)
            {
                throw new DomainValidationException("La fecha de devolucion no puede ser anterior a la de inicio.");
            }

            EndDate = endDate;
            Status = RentalStatus.Returned;
        }
    }
}
