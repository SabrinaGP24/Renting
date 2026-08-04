using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Customer;

namespace HexagonalDDD.Domain.Repositories
{
    public interface ICustomerRepository
    {
        /// <summary>
        /// Devuelve null si el cliente no esta dado de alta todavia. La pantalla de alquiler usa
        /// esto para rellenar los datos si ya existe, o pedirlos si es la primera vez.
        /// </summary>
        Task<CustomerAggregate> GetByDocumentIdAsync(string documentId);

        Task AddAsync(CustomerAggregate customer);

        Task UpdateAsync(CustomerAggregate customer);
    }
}
