using System;
using HexagonalDDD.Domain.Aggregates.Customer;

namespace HexagonalDDD.Applicaion.DTOs
{
    public class CustomerDto
    {
        public string DocumentId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Phone { get; set; }

        public DateTime RegisteredAt { get; set; }

        public static CustomerDto From(CustomerAggregate customer)
        {
            return new CustomerDto
            {
                DocumentId = customer.DocumentId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Phone = customer.Phone,
                RegisteredAt = customer.RegisteredAt
            };
        }
    }
}
