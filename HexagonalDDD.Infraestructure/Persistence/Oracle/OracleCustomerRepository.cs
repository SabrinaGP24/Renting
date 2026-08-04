using System;
using System.Data;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Customer;
using HexagonalDDD.Domain.Repositories;
using Oracle.ManagedDataAccess.Client;

namespace HexagonalDDD.Infraestructure.Persistence.Oracle
{
    /// <summary>
    /// Adaptador Oracle con ADO.NET puro sobre la tabla CUSTOMERS.
    /// La clave primaria es el documento identificativo, que es clave natural del cliente.
    /// </summary>
    public class OracleCustomerRepository : ICustomerRepository
    {
        private const string SelectColumns = "DOCUMENT_ID, FIRST_NAME, LAST_NAME, PHONE, REGISTERED_AT";

        private readonly string _connectionString;

        public OracleCustomerRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("La cadena de conexion de Oracle es obligatoria.", "connectionString");
            }

            _connectionString = connectionString;
        }

        public async Task<CustomerAggregate> GetByDocumentIdAsync(string documentId)
        {
            string normalized = CustomerAggregate.NormalizeDocumentId(documentId);

            using (OracleConnection connection = new OracleConnection(_connectionString))
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + SelectColumns + " FROM CUSTOMERS WHERE DOCUMENT_ID = :p_document";
                command.BindByName = true;
                command.Parameters.Add("p_document", OracleDbType.Varchar2).Value = normalized;

                await connection.OpenAsync();

                using (IDataReader reader = await command.ExecuteReaderAsync())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public async Task AddAsync(CustomerAggregate customer)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "INSERT INTO CUSTOMERS (DOCUMENT_ID, FIRST_NAME, LAST_NAME, PHONE, REGISTERED_AT) " +
                    "VALUES (:p_document, :p_first_name, :p_last_name, :p_phone, :p_registered_at)";
                command.BindByName = true;
                AddCustomerParameters(command, customer);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(CustomerAggregate customer)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "UPDATE CUSTOMERS SET FIRST_NAME = :p_first_name, LAST_NAME = :p_last_name, " +
                    "PHONE = :p_phone, REGISTERED_AT = :p_registered_at WHERE DOCUMENT_ID = :p_document";
                command.BindByName = true;
                AddCustomerParameters(command, customer);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        private static void AddCustomerParameters(OracleCommand command, CustomerAggregate customer)
        {
            command.Parameters.Add("p_document", OracleDbType.Varchar2).Value = customer.DocumentId;
            command.Parameters.Add("p_first_name", OracleDbType.Varchar2).Value = customer.FirstName;
            command.Parameters.Add("p_last_name", OracleDbType.Varchar2).Value = customer.LastName;
            command.Parameters.Add("p_phone", OracleDbType.Varchar2).Value =
                string.IsNullOrEmpty(customer.Phone) ? (object)DBNull.Value : customer.Phone;
            command.Parameters.Add("p_registered_at", OracleDbType.Date).Value = customer.RegisteredAt;
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
