using System;
using System.Threading.Tasks;
using HexagonalDDD.Applicaion.DTOs;
using HexagonalDDD.Domain.Aggregates.Customer;
using HexagonalDDD.Domain.Repositories;

namespace HexagonalDDD.Applicaion.UseCases.Find_Customer
{
    /// <summary>
    /// Busca un cliente por documento. Devuelve null si no existe: no es un error, es el caso
    /// normal de un cliente nuevo, y la pantalla de alquiler lo usa para decidir si rellena los
    /// datos o los pide.
    /// </summary>
    public class FindCustomerHandler
    {
        private readonly ICustomerRepository _customers;

        public FindCustomerHandler(ICustomerRepository customers)
        {
            if (customers == null)
            {
                throw new ArgumentNullException("customers");
            }

            _customers = customers;
        }

        public async Task<CustomerDto> ExecuteAsync(FindCustomerQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }

            string documentId = CustomerAggregate.NormalizeDocumentId(query.DocumentId);
            if (documentId.Length == 0)
            {
                return null;
            }

            CustomerAggregate customer = await _customers.GetByDocumentIdAsync(documentId);

            return customer == null ? null : CustomerDto.From(customer);
        }
    }
}
