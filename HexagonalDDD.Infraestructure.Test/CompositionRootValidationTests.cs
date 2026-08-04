using System;
using System.Threading.Tasks;
using HexagonalDDD.Applicaion.DTOs;
using HexagonalDDD.Applicaion.UseCases.Create_Vehicle;
using HexagonalDDD.Domain.Exceptions;
using HexagonalDDD.Domain.Repositories;
using HexagonalDDD.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WPF_Hexagonal_DDD;

namespace HexagonalDDD.Infraestructure.Test
{
    /// <summary>
    /// Prueba de infraestructura: monta el mismo contenedor de dependencias que construye la capa
    /// de presentacion (Bootstrapper.BuildHost) sin arrancar la interfaz grafica, y ejerce la
    /// entrada de datos de un caso de uso comprobando su validacion.
    /// </summary>
    [TestClass]
    public class CompositionRootValidationTests
    {
        private IHost _host;

        [TestInitialize]
        public void SetUp()
        {
            _host = Bootstrapper.BuildHost();
        }

        [TestCleanup]
        public void TearDown()
        {
            if (_host != null)
            {
                _host.Dispose();
                _host = null;
            }
        }

        [TestMethod]
        public void ElContenedorResuelveTodosLosPuertosYCasosDeUso()
        {
            IServiceProvider services = _host.Services;

            Assert.IsNotNull(services.GetRequiredService<IVehicleRepository>());
            Assert.IsNotNull(services.GetRequiredService<IRentalRepository>());
            Assert.IsNotNull(services.GetRequiredService<IClock>());
            Assert.IsNotNull(services.GetRequiredService<CreateVehicleHandler>());
        }

        [TestMethod]
        public async Task AltaDeVehiculo_ConDatosValidos_DevuelveElVehiculoDisponible()
        {
            CreateVehicleHandler handler = _host.Services.GetRequiredService<CreateVehicleHandler>();
            IClock clock = _host.Services.GetRequiredService<IClock>();

            VehicleDto created = await handler.ExecuteAsync(new CreateVehicleCommand
            {
                PlateNumber = "1234ABC",
                Brand = "Seat",
                Model = "Leon",
                ManufactureDate = clock.Now.AddYears(-2)
            });

            Assert.AreEqual("1234ABC", created.PlateNumber);
            Assert.IsTrue(created.IsAvailable);
        }

        [TestMethod]
        public async Task AltaDeVehiculo_ConMasDeCincoAnosDeAntiguedad_EsRechazada()
        {
            CreateVehicleHandler handler = _host.Services.GetRequiredService<CreateVehicleHandler>();
            IClock clock = _host.Services.GetRequiredService<IClock>();

            VehicleTooOldException exception = await Assert.ThrowsExceptionAsync<VehicleTooOldException>(
                () => handler.ExecuteAsync(new CreateVehicleCommand
                {
                    PlateNumber = "0000ZZZ",
                    Brand = "Renault",
                    Model = "Clio",
                    ManufactureDate = clock.Now.AddYears(-6)
                }));

            Assert.AreEqual("0000ZZZ", exception.PlateNumber);
            Assert.AreEqual(5, exception.MaxAgeInYears);
        }

        [TestMethod]
        public async Task AltaDeVehiculo_SinMatricula_EsRechazada()
        {
            CreateVehicleHandler handler = _host.Services.GetRequiredService<CreateVehicleHandler>();
            IClock clock = _host.Services.GetRequiredService<IClock>();

            await Assert.ThrowsExceptionAsync<DomainValidationException>(
                () => handler.ExecuteAsync(new CreateVehicleCommand
                {
                    PlateNumber = "   ",
                    Brand = "Renault",
                    Model = "Clio",
                    ManufactureDate = clock.Now.AddYears(-1)
                }));
        }

        [TestMethod]
        public async Task AltaDeVehiculo_ConMatriculaRepetida_EsRechazada()
        {
            CreateVehicleHandler handler = _host.Services.GetRequiredService<CreateVehicleHandler>();
            IClock clock = _host.Services.GetRequiredService<IClock>();

            CreateVehicleCommand command = new CreateVehicleCommand
            {
                PlateNumber = "7777AAA",
                Brand = "Ford",
                Model = "Focus",
                ManufactureDate = clock.Now.AddYears(-1)
            };

            await handler.ExecuteAsync(command);

            await Assert.ThrowsExceptionAsync<DomainValidationException>(() => handler.ExecuteAsync(command));
        }
    }
}
