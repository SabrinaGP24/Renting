using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Vehicle;

namespace HexagonalDDD.Domain.Repositories
{
    public interface IVehicleRepository
    {
        Task<VehicleAggregate> GetByIdAsync(Guid id);

        Task<VehicleAggregate> GetByPlateNumberAsync(string plateNumber);

        Task<IReadOnlyCollection<VehicleAggregate>> GetAllAsync();

        Task<IReadOnlyCollection<VehicleAggregate>> GetAvailableAsync();

        Task AddAsync(VehicleAggregate vehicle);

        Task UpdateAsync(VehicleAggregate vehicle);
    }
}
