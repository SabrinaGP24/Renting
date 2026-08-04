using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Vehicle;
using HexagonalDDD.Domain.Repositories;

namespace HexagonalDDD.Unit.Test.Fakes
{
    /// <summary>
    /// Doble de prueba escrito a mano (sin libreria de mocking). Registra las llamadas a UpdateAsync
    /// para poder afirmar que un caso de uso fallido no ha persistido nada.
    /// Los metodos que el caso de uso bajo prueba no debe usar lanzan excepcion a proposito.
    /// </summary>
    internal sealed class FakeVehicleRepository : IVehicleRepository
    {
        private readonly Dictionary<Guid, VehicleAggregate> _vehicles = new Dictionary<Guid, VehicleAggregate>();

        public FakeVehicleRepository()
        {
            UpdatedVehicles = new List<VehicleAggregate>();
        }

        public List<VehicleAggregate> UpdatedVehicles { get; private set; }

        public void Given(VehicleAggregate vehicle)
        {
            _vehicles[vehicle.Id] = vehicle;
        }

        public Task<VehicleAggregate> GetByIdAsync(Guid id)
        {
            VehicleAggregate vehicle;
            return Task.FromResult(_vehicles.TryGetValue(id, out vehicle) ? vehicle : null);
        }

        public Task UpdateAsync(VehicleAggregate vehicle)
        {
            _vehicles[vehicle.Id] = vehicle;
            UpdatedVehicles.Add(vehicle);
            return Task.FromResult(0);
        }

        public Task<VehicleAggregate> GetByPlateNumberAsync(string plateNumber)
        {
            throw new NotSupportedException("RentVehicleHandler no debe consultar por matricula.");
        }

        public Task<IReadOnlyCollection<VehicleAggregate>> GetAllAsync()
        {
            throw new NotSupportedException("RentVehicleHandler no debe listar la flota.");
        }

        public Task<IReadOnlyCollection<VehicleAggregate>> GetAvailableAsync()
        {
            throw new NotSupportedException("RentVehicleHandler no debe listar la flota.");
        }

        public Task AddAsync(VehicleAggregate vehicle)
        {
            throw new NotSupportedException("RentVehicleHandler no debe dar de alta vehiculos.");
        }
    }
}
