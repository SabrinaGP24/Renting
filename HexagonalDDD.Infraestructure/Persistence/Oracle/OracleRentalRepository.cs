using System;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Rental;
using HexagonalDDD.Domain.Repositories;
using Oracle.ManagedDataAccess.Client;

namespace HexagonalDDD.Infraestructure.Persistence.Oracle
{
    /// <summary>
    /// Adaptador Oracle con ADO.NET puro sobre un esquema database-first (tabla RENTALS).
    /// </summary>
    public class OracleRentalRepository : IRentalRepository
    {
        private const string SelectColumns = "ID, VEHICLE_ID, CUSTOMER_DOCUMENT_ID, START_DATE, END_DATE, STATUS";

        private readonly string _connectionString;

        public OracleRentalRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("La cadena de conexion de Oracle es obligatoria.", "connectionString");
            }

            _connectionString = connectionString;
        }

        public async Task<bool> HasActiveRentalAsync(string customerDocumentId)
        {
            string normalized = customerDocumentId == null
                ? string.Empty
                : customerDocumentId.Trim().ToUpperInvariant();

            using (OracleConnection connection = new OracleConnection(_connectionString))
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT COUNT(*) FROM RENTALS WHERE CUSTOMER_DOCUMENT_ID = :p_document AND STATUS = :p_status";
                command.BindByName = true;
                command.Parameters.Add("p_document", OracleDbType.Varchar2).Value = normalized;
                command.Parameters.Add("p_status", OracleDbType.Int32).Value = (int)RentalStatus.Active;

                await connection.OpenAsync();
                object count = await command.ExecuteScalarAsync();

                return Convert.ToInt32(count, CultureInfo.InvariantCulture) > 0;
            }
        }

        public async Task<RentalAggregate> GetActiveByVehicleIdAsync(Guid vehicleId)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT " + SelectColumns + " FROM RENTALS WHERE VEHICLE_ID = :p_vehicle_id AND STATUS = :p_status";
                command.BindByName = true;
                command.Parameters.Add("p_vehicle_id", OracleDbType.Raw).Value = vehicleId.ToByteArray();
                command.Parameters.Add("p_status", OracleDbType.Int32).Value = (int)RentalStatus.Active;

                await connection.OpenAsync();

                using (IDataReader reader = await command.ExecuteReaderAsync())
                {
                    return reader.Read() ? Map(reader) : null;
                }
            }
        }

        public async Task AddAsync(RentalAggregate rental)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "INSERT INTO RENTALS (ID, VEHICLE_ID, CUSTOMER_DOCUMENT_ID, START_DATE, END_DATE, STATUS) " +
                    "VALUES (:p_id, :p_vehicle_id, :p_document, :p_start_date, :p_end_date, :p_status)";
                command.BindByName = true;
                AddRentalParameters(command, rental);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(RentalAggregate rental)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "UPDATE RENTALS SET VEHICLE_ID = :p_vehicle_id, CUSTOMER_DOCUMENT_ID = :p_document, " +
                    "START_DATE = :p_start_date, END_DATE = :p_end_date, STATUS = :p_status WHERE ID = :p_id";
                command.BindByName = true;
                AddRentalParameters(command, rental);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        private static void AddRentalParameters(OracleCommand command, RentalAggregate rental)
        {
            command.Parameters.Add("p_id", OracleDbType.Raw).Value = rental.Id.ToByteArray();
            command.Parameters.Add("p_vehicle_id", OracleDbType.Raw).Value = rental.VehicleId.ToByteArray();
            command.Parameters.Add("p_document", OracleDbType.Varchar2).Value = rental.CustomerDocumentId;
            command.Parameters.Add("p_start_date", OracleDbType.Date).Value = rental.StartDate;
            command.Parameters.Add("p_end_date", OracleDbType.Date).Value =
                rental.EndDate.HasValue ? (object)rental.EndDate.Value : DBNull.Value;
            command.Parameters.Add("p_status", OracleDbType.Int32).Value = (int)rental.Status;
        }

        private static RentalAggregate Map(IDataReader reader)
        {
            return RentalAggregate.Rehydrate(
                new Guid((byte[])reader.GetValue(0)),
                new Guid((byte[])reader.GetValue(1)),
                reader.GetString(2),
                reader.GetDateTime(3),
                reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                (RentalStatus)Convert.ToInt32(reader.GetValue(5), CultureInfo.InvariantCulture));
        }
    }
}
