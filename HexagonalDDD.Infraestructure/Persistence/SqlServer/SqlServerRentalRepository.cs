using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Rental;
using HexagonalDDD.Domain.Repositories;

namespace HexagonalDDD.Infraestructure.Persistence.SqlServer
{
    public class SqlServerRentalRepository : IRentalRepository
    {
        private const string SelectColumns = "ID, VEHICLE_ID, CUSTOMER_DOCUMENT_ID, START_DATE, END_DATE, STATUS";

        private readonly string _connectionString;

        public SqlServerRentalRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("La cadena de conexion de SQL Server es obligatoria.", "connectionString");
            }

            _connectionString = connectionString;
        }

        public async Task<bool> HasActiveRentalAsync(string customerDocumentId)
        {
            string normalized = customerDocumentId == null
                ? string.Empty
                : customerDocumentId.Trim().ToUpperInvariant();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT COUNT(*) FROM RENTALS WHERE CUSTOMER_DOCUMENT_ID = @p_document AND STATUS = @p_status";
                command.Parameters.Add("@p_document", SqlDbType.NVarChar, 20).Value = normalized;
                command.Parameters.Add("@p_status", SqlDbType.TinyInt).Value = (int)RentalStatus.Active;

                await connection.OpenAsync();
                object count = await command.ExecuteScalarAsync();

                return Convert.ToInt32(count, CultureInfo.InvariantCulture) > 0;
            }
        }

        public async Task<RentalAggregate> GetActiveByVehicleIdAsync(Guid vehicleId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT " + SelectColumns + " FROM RENTALS WHERE VEHICLE_ID = @p_vehicle_id AND STATUS = @p_status";
                command.Parameters.Add("@p_vehicle_id", SqlDbType.UniqueIdentifier).Value = vehicleId;
                command.Parameters.Add("@p_status", SqlDbType.TinyInt).Value = (int)RentalStatus.Active;

                await connection.OpenAsync();

                using (IDataReader reader = await command.ExecuteReaderAsync())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public async Task AddAsync(RentalAggregate rental)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "INSERT INTO RENTALS (ID, VEHICLE_ID, CUSTOMER_DOCUMENT_ID, START_DATE, END_DATE, STATUS) " +
                    "VALUES (@p_id, @p_vehicle_id, @p_document, @p_start_date, @p_end_date, @p_status)";
                AddRentalParameters(command, rental);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(RentalAggregate rental)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "UPDATE RENTALS SET VEHICLE_ID = @p_vehicle_id, CUSTOMER_DOCUMENT_ID = @p_document, " +
                    "START_DATE = @p_start_date, END_DATE = @p_end_date, STATUS = @p_status WHERE ID = @p_id";
                AddRentalParameters(command, rental);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        private static void AddRentalParameters(SqlCommand command, RentalAggregate rental)
        {
            command.Parameters.Add("@p_id", SqlDbType.UniqueIdentifier).Value = rental.Id;
            command.Parameters.Add("@p_vehicle_id", SqlDbType.UniqueIdentifier).Value = rental.VehicleId;
            command.Parameters.Add("@p_document", SqlDbType.NVarChar, 20).Value = rental.CustomerDocumentId;
            command.Parameters.Add("@p_start_date", SqlDbType.DateTime2).Value = rental.StartDate;
            command.Parameters.Add("@p_end_date", SqlDbType.DateTime2).Value =
                rental.EndDate.HasValue ? (object)rental.EndDate.Value : DBNull.Value;
            command.Parameters.Add("@p_status", SqlDbType.TinyInt).Value = (int)rental.Status;
        }

        private static RentalAggregate Map(IDataReader reader)
        {
            return RentalAggregate.Rehydrate(
                reader.GetGuid(0),
                reader.GetGuid(1),
                reader.GetString(2),
                reader.GetDateTime(3),
                reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                (RentalStatus)Convert.ToInt32(reader.GetValue(5), CultureInfo.InvariantCulture));
        }
    }
}
