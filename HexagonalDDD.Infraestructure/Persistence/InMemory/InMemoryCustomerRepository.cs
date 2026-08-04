using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Customer;
using HexagonalDDD.Domain.Repositories;

namespace HexagonalDDD.Infraestructure.Persistence.InMemory
{
    public class InMemoryCustomerRepository : ICustomerRepository
    {
        private readonly ConcurrentDictionary<string, CustomerAggregate> _customers =
            new ConcurrentDictionary<string, CustomerAggregate>(StringComparer.OrdinalIgnoreCase);

        public Task<CustomerAggregate> GetByDocumentIdAsync(string documentId)
        {
            string key = CustomerAggregate.NormalizeDocumentId(documentId);

            CustomerAggregate customer;
            return Task.FromResult(_customers.TryGetValue(key, out customer) ? Clone(customer) : null);
        }

        public Task AddAsync(CustomerAggregate customer)
        {
            _customers[customer.DocumentId] = Clone(customer);
            return Task.FromResult(0);
        }

        public Task UpdateAsync(CustomerAggregate customer)
        {
            _customers[customer.DocumentId] = Clone(customer);
            return Task.FromResult(0);
        }

        private static CustomerAggregate Clone(CustomerAggregate customer)
        {
            return CustomerAggregate.Rehydrate(
                customer.DocumentId,
                customer.FirstName,
                customer.LastName,
                customer.Phone,
                customer.RegisteredAt);
        }
    }
}
