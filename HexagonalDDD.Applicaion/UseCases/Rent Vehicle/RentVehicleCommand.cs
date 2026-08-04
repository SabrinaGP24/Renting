using System;

namespace HexagonalDDD.Applicaion.UseCases.Rent_Vehicle
{
    public class RentVehicleCommand
    {
        public Guid VehicleId { get; set; }

        public string CustomerDocumentId { get; set; }

        /// <summary>
        /// Datos de contacto del cliente. Si el documento no existe todavia, se da de alta con
        /// ellos; si ya existe, se actualizan. La pantalla de alquiler los precarga cuando el
        /// cliente ya es conocido.
        /// </summary>
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Phone { get; set; }
    }
}
