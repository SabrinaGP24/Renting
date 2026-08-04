using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Vehicle;
using HexagonalDDD.Domain.Repositories;

namespace HexagonalDDD.Infraestructure.Persistence.InMemory
{
    /// <summary>
    /// Adaptador de persistencia en memoria: permite ejecutar y testear la aplicacion sin
    /// ninguna dependencia externa. Se registra como singleton para que el estado sobreviva
    /// entre casos de uso.
    /// </summary>
    public class InMemoryVehicleRepository : IVehicleRepository
    {
        private readonly ConcurrentDictionary<Guid, VehicleAggregate> _vehicles =
            new ConcurrentDictionary<Guid, VehicleAggregate>();

        public Task<VehicleAggregate> GetByIdAsync(Guid id)
        {
            VehicleAggregate vehicle;
            return Task.FromResult(_vehicles.TryGetValue(id, out vehicle) ? Clone(vehicle) : null);
        }

        public Task<VehicleAggregate> GetByPlateNumberAsync(string plateNumber)
        {
            string normalized = plateNumber == null ? string.Empty : plateNumber.Trim().ToUpperInvariant();

            VehicleAggregate match = _vehicles.Values
                .FirstOrDefault(v => string.Equals(v.PlateNumber, normalized, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(match == null ? null : Clone(match));
        }

        public Task<IReadOnlyCollection<VehicleAggregate>> GetAllAsync()
        {
            IReadOnlyCollection<VehicleAggregate> result = _vehicles.Values.Select(Clone).ToList();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyCollection<VehicleAggregate>> GetAvailableAsync()
        {
            IReadOnlyCollection<VehicleAggregate> result = _vehicles.Values
                .Where(v => v.Status == VehicleStatus.Available)
                .Select(Clone)
                .ToList();

            return Task.FromResult(result);
        }

        public Task AddAsync(VehicleAggregate vehicle)
        {
            _vehicles[vehicle.Id] = Clone(vehicle);
            return Task.FromResult(0);
        }

        public Task UpdateAsync(VehicleAggregate vehicle)
        {
            _vehicles[vehicle.Id] = Clone(vehicle);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Se devuelven copias para imitar a una base de datos real: los cambios hechos sobre un
        /// agregado leido no se persisten hasta que se llama explicitamente a UpdateAsync.
        /// </summary>
        private static VehicleAggregate Clone(VehicleAggregate vehicle)
        {
            return VehicleAggregate.Rehydrate(
                vehicle.Id,
                vehicle.PlateNumber,
                vehicle.Brand,
                vehicle.Model,
                vehicle.ManufactureDate,
                vehicle.Status);
        }
    }
}
