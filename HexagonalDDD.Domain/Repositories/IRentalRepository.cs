using System;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Rental;

namespace HexagonalDDD.Domain.Repositories
{
    public interface IRentalRepository
    {
        /// <summary>
        /// Soporta la regla "un cliente no puede tener mas de un vehiculo alquilado a la vez".
        /// Vive en el repositorio porque la regla abarca un conjunto de agregados, no uno solo.
        /// </summary>
        Task<bool> HasActiveRentalAsync(string customerDocumentId);

        Task<RentalAggregate> GetActiveByVehicleIdAsync(Guid vehicleId);

        Task AddAsync(RentalAggregate rental);

        Task UpdateAsync(RentalAggregate rental);
    }
}
