using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Customer;
using HexagonalDDD.Domain.Repositories;

namespace HexagonalDDD.Infraestructure.Persistence.SqlServer
{
    public class SqlServerCustomerRepository : ICustomerRepository
    {
        private const string SelectColumns = "DOCUMENT_ID, FIRST_NAME, LAST_NAME, PHONE, REGISTERED_AT";

        private readonly string _connectionString;

        public SqlServerCustomerRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("La cadena de conexion de SQL Server es obligatoria.", "connectionString");
            }

            _connectionString = connectionString;
        }

        public async Task<CustomerAggregate> GetByDocumentIdAsync(string documentId)
        {
            string normalized = CustomerAggregate.NormalizeDocumentId(documentId);

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + SelectColumns + " FROM CUSTOMERS WHERE DOCUMENT_ID = @p_document";
                command.Parameters.Add("@p_document", SqlDbType.NVarChar, 20).Value = normalized;

                await connection.OpenAsync();

                using (IDataReader reader = await command.ExecuteReaderAsync())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public async Task AddAsync(CustomerAggregate customer)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "INSERT INTO CUSTOMERS (DOCUMENT_ID, FIRST_NAME, LAST_NAME, PHONE, REGISTERED_AT) " +
                    "VALUES (@p_document, @p_first_name, @p_last_name, @p_phone, @p_registered_at)";
                AddCustomerParameters(command, customer);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(CustomerAggregate customer)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "UPDATE CUSTOMERS SET FIRST_NAME = @p_first_name, LAST_NAME = @p_last_name, " +
                    "PHONE = @p_phone, REGISTERED_AT = @p_registered_at WHERE DOCUMENT_ID = @p_document";
                AddCustomerParameters(command, customer);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        private static void AddCustomerParameters(SqlCommand command, CustomerAggregate customer)
        {
            command.Parameters.Add("@p_document", SqlDbType.NVarChar, 20).Value = customer.DocumentId;
            command.Parameters.Add("@p_first_name", SqlDbType.NVarChar, 50).Value = customer.FirstName;
            command.Parameters.Add("@p_last_name", SqlDbType.NVarChar, 80).Value = customer.LastName;
            command.Parameters.Add("@p_phone", SqlDbType.NVarChar, 20).Value =
                string.IsNullOrEmpty(customer.Phone) ? (object)DBNull.Value : customer.Phone;
            command.Parameters.Add("@p_registered_at", SqlDbType.DateTime2).Value = customer.RegisteredAt;
        }

        private static CustomerAggregate Map(IDataReader reader)
        {
            return CustomerAggregate.Rehydrate(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                reader.GetDateTime(4));
        }
    }
}
