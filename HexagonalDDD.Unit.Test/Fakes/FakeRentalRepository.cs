using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Rental;
using HexagonalDDD.Domain.Repositories;

namespace HexagonalDDD.Unit.Test.Fakes
{
    internal sealed class FakeRentalRepository : IRentalRepository
    {
        private readonly HashSet<string> _customersWithActiveRental =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public FakeRentalRepository()
        {
            AddedRentals = new List<RentalAggregate>();
            RequestedCustomerDocumentIds = new List<string>();
        }

        public List<RentalAggregate> AddedRentals { get; private set; }

        public List<string> RequestedCustomerDocumentIds { get; private set; }

        public void GivenActiveRentalFor(string customerDocumentId)
        {
            _customersWithActiveRental.Add(customerDocumentId);
        }

        public Task<bool> HasActiveRentalAsync(string customerDocumentId)
        {
            RequestedCustomerDocumentIds.Add(customerDocumentId);
            return Task.FromResult(_customersWithActiveRental.Contains(customerDocumentId));
        }

        public Task AddAsync(RentalAggregate rental)
        {
            AddedRentals.Add(rental);
            _customersWithActiveRental.Add(rental.CustomerDocumentId);
            return Task.FromResult(0);
        }

        public Task<RentalAggregate> GetActiveByVehicleIdAsync(Guid vehicleId)
        {
            throw new NotSupportedException("RentVehicleHandler no debe consultar alquileres por vehiculo.");
        }

        public Task UpdateAsync(RentalAggregate rental)
        {
            throw new NotSupportedException("RentVehicleHandler no debe actualizar alquileres.");
        }
    }
}
