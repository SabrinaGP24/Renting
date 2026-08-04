using System;
using HexagonalDDD.Domain.Aggregates.Vehicle;

namespace HexagonalDDD.Applicaion.DTOs
{
    public class VehicleDto
    {
        public Guid Id { get; set; }

        public string PlateNumber { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public DateTime ManufactureDate { get; set; }

        public string Status { get; set; }

        public bool IsAvailable { get; set; }

        public static VehicleDto From(VehicleAggregate vehicle)
        {
            return new VehicleDto
            {
                Id = vehicle.Id,
                PlateNumber = vehicle.PlateNumber,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                ManufactureDate = vehicle.ManufactureDate,
                Status = vehicle.Status.ToString(),
                IsAvailable = vehicle.Status == VehicleStatus.Available
            };
        }
    }
}
