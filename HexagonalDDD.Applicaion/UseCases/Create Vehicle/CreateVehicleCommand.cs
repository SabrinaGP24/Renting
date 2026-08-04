using System;

namespace HexagonalDDD.Applicaion.UseCases.Create_Vehicle
{
    public class CreateVehicleCommand
    {
        public string PlateNumber { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public DateTime ManufactureDate { get; set; }
    }
}
