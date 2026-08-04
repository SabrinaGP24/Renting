using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HexagonalDDD.Applicaion.DependencyInjection;
using HexagonalDDD.Applicaion.DTOs;
using HexagonalDDD.Applicaion.UseCases.Create_Vehicle;
using HexagonalDDD.Applicaion.UseCases.List_Vehicles;
using HexagonalDDD.Applicaion.UseCases.Rent_Vehicle;
using HexagonalDDD.Applicaion.UseCases.Return_Vehicle;
using HexagonalDDD.Domain.Exceptions;
using HexagonalDDD.Domain.Services;
using HexagonalDDD.Infraestructure.Configuration;
using HexagonalDDD.Infraestructure.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace HexagonalDDD.Functional.Test
{
    /// <summary>
    /// Prueba funcional: recorre el ciclo completo de negocio atravesando aplicacion, dominio e
    /// infraestructura reales, pero sin levantar el host de escritorio (nada de WPF ni de App.xaml).
    /// Se usan los adaptadores en memoria, de modo que no hace falta Oracle ni SQL Server.
    /// </summary>
    [TestFixture]
    public class RentingFlowTests
    {
        private const string Customer = "12345678Z";

        private ServiceProvider _services;
        private CreateVehicleHandler _createVehicle;
        private ListAvailableVehiclesHandler _listAvailableVehicles;
        private ListAllVehiclesHandler _listAllVehicles;
        private RentVehicleHandler _rentVehicle;
        private ReturnVehicleHandler _returnVehicle;
        private IClock _clock;

        [SetUp]
        public void SetUp()
        {
            // Mismo ensamblado de capas que la aplicacion real, pero forzando persistencia en memoria.
            IServiceCollection services = new ServiceCollection();
            services.AddInfrastructure(PersistenceOptions.InMemory());
            services.AddApplication();

            _services = services.BuildServiceProvider();

            _createVehicle = _services.GetRequiredService<CreateVehicleHandler>();
            _listAvailableVehicles = _services.GetRequiredService<ListAvailableVehiclesHandler>();
            _listAllVehicles = _services.GetRequiredService<ListAllVehiclesHandler>();
            _rentVehicle = _services.GetRequiredService<RentVehicleHandler>();
            _returnVehicle = _services.GetRequiredService<ReturnVehicleHandler>();
            _clock = _services.GetRequiredService<IClock>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_services != null)
            {
                _services.Dispose();
                _services = null;
            }
        }

        [Test]
        public async Task CicloCompleto_AltaAlquilerDevolucionYNuevoAlquiler()
        {
            VehicleDto first = await CreateVehicleAsync("1111AAA");
            VehicleDto second = await CreateVehicleAsync("2222BBB");

            Assert.That(await AvailablePlatesAsync(), Is.EquivalentTo(new[] { "1111AAA", "2222BBB" }));

            // 1. El cliente alquila el primer vehiculo.
            RentalDto rental = await _rentVehicle.ExecuteAsync(new RentVehicleCommand
            {
                VehicleId = first.Id,
                CustomerDocumentId = Customer
            });

            Assert.That(rental.CustomerDocumentId, Is.EqualTo(Customer));
            Assert.That(rental.EndDate, Is.Null);
            Assert.That(await AvailablePlatesAsync(), Is.EquivalentTo(new[] { "2222BBB" }));

            // 2. El mismo cliente no puede alquilar un segundo vehiculo.
            Func<Task> rentSecondVehicle = () => _rentVehicle.ExecuteAsync(new RentVehicleCommand
            {
                VehicleId = second.Id,
                CustomerDocumentId = Customer
            });

            Assert.ThrowsAsync<CustomerAlreadyHasActiveRentalException>(rentSecondVehicle);

            Assert.That(await AvailablePlatesAsync(), Is.EquivalentTo(new[] { "2222BBB" }),
                "El intento fallido no debe cambiar el estado de la flota.");

            // 3. Devuelve el primero.
            RentalDto returned = await _returnVehicle.ExecuteAsync(new ReturnVehicleCommand
            {
                VehicleId = first.Id
            });

            Assert.That(returned.Id, Is.EqualTo(rental.Id));
            Assert.That(returned.Status, Is.EqualTo("Returned"));
            Assert.That(returned.EndDate, Is.Not.Null);

            // 4. Y ahora si puede alquilar el segundo.
            RentalDto secondRental = await _rentVehicle.ExecuteAsync(new RentVehicleCommand
            {
                VehicleId = second.Id,
                CustomerDocumentId = Customer
            });

            Assert.That(secondRental.Id, Is.Not.EqualTo(rental.Id));
            Assert.That(await AvailablePlatesAsync(), Is.EquivalentTo(new[] { "1111AAA" }));
        }

        [Test]
        public async Task DosClientesDistintos_PuedenTenerUnVehiculoCadaUno()
        {
            VehicleDto first = await CreateVehicleAsync("3333CCC");
            VehicleDto second = await CreateVehicleAsync("4444DDD");

            await _rentVehicle.ExecuteAsync(new RentVehicleCommand
            {
                VehicleId = first.Id,
                CustomerDocumentId = "11111111A"
            });

            await _rentVehicle.ExecuteAsync(new RentVehicleCommand
            {
                VehicleId = second.Id,
                CustomerDocumentId = "22222222B"
            });

            Assert.That(await AvailablePlatesAsync(), Is.Empty);

            IReadOnlyCollection<VehicleDto> all = await _listAllVehicles.ExecuteAsync(new ListAllVehiclesQuery());
            Assert.That(all.Count, Is.EqualTo(2));
            Assert.That(all.All(v => !v.IsAvailable), Is.True);
        }

        [Test]
        public void AltaDeVehiculo_ConMasDeCincoAnosDeAntiguedad_EsRechazada()
        {
            Func<Task> createTooOldVehicle = () => _createVehicle.ExecuteAsync(new CreateVehicleCommand
            {
                PlateNumber = "9999ZZZ",
                Brand = "Renault",
                Model = "Clio",
                ManufactureDate = _clock.Now.AddYears(-6)
            });

            Assert.ThrowsAsync<VehicleTooOldException>(createTooOldVehicle);
        }

        [Test]
        public async Task Devolver_UnVehiculoQueNoEstaAlquilado_EsRechazado()
        {
            VehicleDto vehicle = await CreateVehicleAsync("5555EEE");

            Func<Task> returnNotRentedVehicle =
                () => _returnVehicle.ExecuteAsync(new ReturnVehicleCommand { VehicleId = vehicle.Id });

            Assert.ThrowsAsync<ActiveRentalNotFoundException>(returnNotRentedVehicle);
        }

        private async Task<VehicleDto> CreateVehicleAsync(string plateNumber)
        {
            return await _createVehicle.ExecuteAsync(new CreateVehicleCommand
            {
                PlateNumber = plateNumber,
                Brand = "Seat",
                Model = "Leon",
                ManufactureDate = _clock.Now.AddYears(-2)
            });
        }

        private async Task<string[]> AvailablePlatesAsync()
        {
            IReadOnlyCollection<VehicleDto> available =
                await _listAvailableVehicles.ExecuteAsync(new ListAvailableVehiclesQuery());

            return available.Select(v => v.PlateNumber).ToArray();
        }
    }
}
