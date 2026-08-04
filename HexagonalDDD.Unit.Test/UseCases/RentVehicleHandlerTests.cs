using System;
using System.Threading.Tasks;
using HexagonalDDD.Applicaion.DTOs;
using HexagonalDDD.Applicaion.UseCases.Rent_Vehicle;
using HexagonalDDD.Domain.Aggregates.Vehicle;
using HexagonalDDD.Domain.Exceptions;
using HexagonalDDD.Unit.Test.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HexagonalDDD.Unit.Test.UseCases
{
    /// <summary>
    /// Prueba unitaria pura: el caso de uso se ejerce contra dobles escritos a mano, sin base de
    /// datos, sin contenedor de dependencias y sin reloj del sistema.
    /// </summary>
    [TestClass]
    public class RentVehicleHandlerTests
    {
        private static readonly DateTime Today = new DateTime(2025, 6, 15);

        private FakeVehicleRepository _vehicles;
        private FakeRentalRepository _rentals;
        private FakeCustomerRepository _customers;
        private RentVehicleHandler _handler;

        [TestInitialize]
        public void SetUp()
        {
            _vehicles = new FakeVehicleRepository();
            _rentals = new FakeRentalRepository();
            _customers = new FakeCustomerRepository();
            _handler = new RentVehicleHandler(_vehicles, _rentals, _customers, new FixedClock(Today));
        }

        private static RentVehicleCommand CommandFor(Guid vehicleId, string documentId)
        {
            return new RentVehicleCommand
            {
                VehicleId = vehicleId,
                CustomerDocumentId = documentId,
                FirstName = "Ana",
                LastName = "Garcia Lopez",
                Phone = "600123456"
            };
        }

        [TestMethod]
        public async Task Alquilar_CuandoElClienteNoTieneNingunAlquilerActivo_RegistraElAlquilerYMarcaElVehiculoComoAlquilado()
        {
            VehicleAggregate vehicle = GivenAvailableVehicle("1234ABC");

            RentalDto rental = await _handler.ExecuteAsync(CommandFor(vehicle.Id, "12345678Z"));

            Assert.AreEqual("12345678Z", rental.CustomerDocumentId);
            Assert.AreEqual(Today, rental.StartDate);
            Assert.IsNull(rental.EndDate);
            Assert.AreEqual(1, _rentals.AddedRentals.Count);

            Assert.AreEqual(1, _vehicles.UpdatedVehicles.Count, "El vehiculo debe persistirse como alquilado.");
            Assert.AreEqual(VehicleStatus.Rented, _vehicles.UpdatedVehicles[0].Status);

            Assert.AreEqual(1, _customers.AddedCustomers.Count, "El cliente nuevo debe darse de alta.");
            Assert.AreEqual("Ana", _customers.AddedCustomers[0].FirstName);
        }

        [TestMethod]
        public async Task Alquilar_CuandoElClienteYaTieneUnVehiculo_LanzaCustomerAlreadyHasActiveRentalException()
        {
            VehicleAggregate vehicle = GivenAvailableVehicle("5678DEF");
            _rentals.GivenActiveRentalFor("12345678Z");

            await Assert.ThrowsExceptionAsync<CustomerAlreadyHasActiveRentalException>(
                () => _handler.ExecuteAsync(CommandFor(vehicle.Id, "12345678Z")));

            Assert.AreEqual(0, _rentals.AddedRentals.Count, "No debe registrarse ningun alquiler.");
            Assert.AreEqual(0, _vehicles.UpdatedVehicles.Count, "El vehiculo no debe quedar marcado como alquilado.");
            Assert.AreEqual(0, _customers.AddedCustomers.Count, "Tampoco debe darse de alta el cliente.");
        }

        [TestMethod]
        public async Task Alquilar_NormalizaElDocumentoDelCliente_ParaQueLaReglaDeUnAlquilerPorPersonaNoSeSalteConMinusculas()
        {
            VehicleAggregate vehicle = GivenAvailableVehicle("9012GHI");
            _rentals.GivenActiveRentalFor("12345678Z");

            await Assert.ThrowsExceptionAsync<CustomerAlreadyHasActiveRentalException>(
                () => _handler.ExecuteAsync(CommandFor(vehicle.Id, "  12345678z  ")));

            CollectionAssert.Contains(_rentals.RequestedCustomerDocumentIds, "12345678Z");
        }

        [TestMethod]
        public async Task Alquilar_CuandoElVehiculoYaEstaAlquilado_LanzaVehicleNotAvailableException()
        {
            VehicleAggregate vehicle = GivenAvailableVehicle("3456JKL");
            vehicle.Rent();

            await Assert.ThrowsExceptionAsync<VehicleNotAvailableException>(
                () => _handler.ExecuteAsync(CommandFor(vehicle.Id, "87654321X")));

            Assert.AreEqual(0, _rentals.AddedRentals.Count);
        }

        [TestMethod]
        public async Task Alquilar_CuandoElVehiculoNoExiste_LanzaVehicleNotFoundException()
        {
            await Assert.ThrowsExceptionAsync<VehicleNotFoundException>(
                () => _handler.ExecuteAsync(CommandFor(Guid.NewGuid(), "87654321X")));
        }

        private VehicleAggregate GivenAvailableVehicle(string plateNumber)
        {
            VehicleAggregate vehicle = VehicleAggregate.Create(
                Guid.NewGuid(),
                plateNumber,
                "Seat",
                "Leon",
                Today.AddYears(-2),
                Today);

            _vehicles.Given(vehicle);
            return vehicle;
        }
    }
}
