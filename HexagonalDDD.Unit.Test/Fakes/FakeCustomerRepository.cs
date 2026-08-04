using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Customer;
using HexagonalDDD.Domain.Repositories;

namespace HexagonalDDD.Unit.Test.Fakes
{
    internal sealed class FakeCustomerRepository : ICustomerRepository
    {
        private readonly Dictionary<string, CustomerAggregate> _customers =
            new Dictionary<string, CustomerAggregate>(StringComparer.OrdinalIgnoreCase);

        public FakeCustomerRepository()
        {
            AddedCustomers = new List<CustomerAggregate>();
            UpdatedCustomers = new List<CustomerAggregate>();
        }

        public List<CustomerAggregate> AddedCustomers { get; private set; }

        public List<CustomerAggregate> UpdatedCustomers { get; private set; }

        public void Given(CustomerAggregate customer)
        {
            _customers[customer.DocumentId] = customer;
        }

        public Task<CustomerAggregate> GetByDocumentIdAsync(string documentId)
        {
            CustomerAggregate customer;
            return Task.FromResult(_customers.TryGetValue(documentId, out customer) ? customer : null);
        }

        public Task AddAsync(CustomerAggregate customer)
        {
            _customers[customer.DocumentId] = customer;
            AddedCustomers.Add(customer);
            return Task.FromResult(0);
        }

        public Task UpdateAsync(CustomerAggregate customer)
        {
            _customers[customer.DocumentId] = customer;
            UpdatedCustomers.Add(customer);
            return Task.FromResult(0);
        }
    }
}
