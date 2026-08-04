using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Rental;
using HexagonalDDD.Domain.Repositories;

namespace HexagonalDDD.Infraestructure.Persistence.InMemory
{
    public class InMemoryRentalRepository : IRentalRepository
    {
        private readonly ConcurrentDictionary<Guid, RentalAggregate> _rentals =
            new ConcurrentDictionary<Guid, RentalAggregate>();

        public Task<bool> HasActiveRentalAsync(string customerDocumentId)
        {
            string normalized = customerDocumentId == null
                ? string.Empty
                : customerDocumentId.Trim().ToUpperInvariant();

            bool exists = _rentals.Values.Any(r =>
                r.Status == RentalStatus.Active &&
                string.Equals(r.CustomerDocumentId, normalized, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(exists);
        }

        public Task<RentalAggregate> GetActiveByVehicleIdAsync(Guid vehicleId)
        {
            RentalAggregate match = _rentals.Values
                .FirstOrDefault(r => r.VehicleId == vehicleId && r.Status == RentalStatus.Active);

            return Task.FromResult(match == null ? null : Clone(match));
        }

        public Task AddAsync(RentalAggregate rental)
        {
            _rentals[rental.Id] = Clone(rental);
            return Task.FromResult(0);
        }

        public Task UpdateAsync(RentalAggregate rental)
        {
            _rentals[rental.Id] = Clone(rental);
            return Task.FromResult(0);
        }

        private static RentalAggregate Clone(RentalAggregate rental)
        {
            return RentalAggregate.Rehydrate(
                rental.Id,
                rental.VehicleId,
                rental.CustomerDocumentId,
                rental.StartDate,
                rental.EndDate,
                rental.Status);
        }
    }
}
