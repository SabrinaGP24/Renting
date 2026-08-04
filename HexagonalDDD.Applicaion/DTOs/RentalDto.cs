using System;
using HexagonalDDD.Domain.Aggregates.Rental;

namespace HexagonalDDD.Applicaion.DTOs
{
    public class RentalDto
    {
        public Guid Id { get; set; }

        public Guid VehicleId { get; set; }

        public string CustomerDocumentId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Status { get; set; }

        public static RentalDto From(RentalAggregate rental)
        {
            return new RentalDto
            {
                Id = rental.Id,
                VehicleId = rental.VehicleId,
                CustomerDocumentId = rental.CustomerDocumentId,
                StartDate = rental.StartDate,
                EndDate = rental.EndDate,
                Status = rental.Status.ToString()
            };
        }
    }
}
