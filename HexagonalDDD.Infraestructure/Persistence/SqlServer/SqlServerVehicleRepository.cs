using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Vehicle;
using HexagonalDDD.Domain.Repositories;

namespace HexagonalDDD.Infraestructure.Persistence.SqlServer
{
    /// <summary>
    /// Adaptador SQL Server con ADO.NET puro sobre el mismo esquema database-first que Oracle.
    /// Existe como alternativa para poder ejecutar la aplicacion contra una base de datos real
    /// (SQL Server Management Studio / LocalDB) cuando no se dispone de una instancia Oracle.
    /// </summary>
    public class SqlServerVehicleRepository : IVehicleRepository
    {
        private const string SelectColumns = "ID, PLATE_NUMBER, BRAND, MODEL, MANUFACTURE_DATE, STATUS";

        private readonly string _connectionString;

        public SqlServerVehicleRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("La cadena de conexion de SQL Server es obligatoria.", "connectionString");
            }

            _connectionString = connectionString;
        }

        public async Task<VehicleAggregate> GetByIdAsync(Guid id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + SelectColumns + " FROM VEHICLES WHERE ID = @p_id";
                command.Parameters.Add("@p_id", SqlDbType.UniqueIdentifier).Value = id;

                await connection.OpenAsync();
                return await ReadSingleAsync(command);
            }
        }

        public async Task<VehicleAggregate> GetByPlateNumberAsync(string plateNumber)
        {
            string normalized = plateNumber == null ? string.Empty : plateNumber.Trim().ToUpperInvariant();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + SelectColumns + " FROM VEHICLES WHERE PLATE_NUMBER = @p_plate";
                command.Parameters.Add("@p_plate", SqlDbType.NVarChar, 20).Value = normalized;

                await connection.OpenAsync();
                return await ReadSingleAsync(command);
            }
        }

        public async Task<IReadOnlyCollection<VehicleAggregate>> GetAllAsync()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + SelectColumns + " FROM VEHICLES ORDER BY PLATE_NUMBER";

                await connection.OpenAsync();
                return await ReadManyAsync(command);
            }
        }

        public async Task<IReadOnlyCollection<VehicleAggregate>> GetAvailableAsync()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT " + SelectColumns + " FROM VEHICLES WHERE STATUS = @p_status ORDER BY PLATE_NUMBER";
                command.Parameters.Add("@p_status", SqlDbType.TinyInt).Value = (int)VehicleStatus.Available;

                await connection.OpenAsync();
                return await ReadManyAsync(command);
            }
        }

        public async Task AddAsync(VehicleAggregate vehicle)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "INSERT INTO VEHICLES (ID, PLATE_NUMBER, BRAND, MODEL, MANUFACTURE_DATE, STATUS) " +
                    "VALUES (@p_id, @p_plate, @p_brand, @p_model, @p_manufacture_date, @p_status)";
                AddVehicleParameters(command, vehicle);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(VehicleAggregate vehicle)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "UPDATE VEHICLES SET PLATE_NUMBER = @p_plate, BRAND = @p_brand, MODEL = @p_model, " +
                    "MANUFACTURE_DATE = @p_manufacture_date, STATUS = @p_status WHERE ID = @p_id";
                AddVehicleParameters(command, vehicle);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        private static void AddVehicleParameters(SqlCommand command, VehicleAggregate vehicle)
        {
            command.Parameters.Add("@p_id", SqlDbType.UniqueIdentifier).Value = vehicle.Id;
            command.Parameters.Add("@p_plate", SqlDbType.NVarChar, 20).Value = vehicle.PlateNumber;
            command.Parameters.Add("@p_brand", SqlDbType.NVarChar, 50).Value = vehicle.Brand;
            command.Parameters.Add("@p_model", SqlDbType.NVarChar, 50).Value = vehicle.Model;
            command.Parameters.Add("@p_manufacture_date", SqlDbType.Date).Value = vehicle.ManufactureDate;
            command.Parameters.Add("@p_status", SqlDbType.TinyInt).Value = (int)vehicle.Status;
        }

        private static async Task<VehicleAggregate> ReadSingleAsync(SqlCommand command)
        {
            using (IDataReader reader = await command.ExecuteReaderAsync())
            {
                return reader.Read() ? Map(reader) : null;
            }
        }

        private static async Task<IReadOnlyCollection<VehicleAggregate>> ReadManyAsync(SqlCommand command)
        {
            List<VehicleAggregate> vehicles = new List<VehicleAggregate>();

            using (IDataReader reader = await command.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    vehicles.Add(Map(reader));
                }
            }

            return vehicles;
        }

        private static VehicleAggregate Map(IDataReader reader)
        {
            return VehicleAggregate.Rehydrate(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetDateTime(4),
                (VehicleStatus)Convert.ToInt32(reader.GetValue(5), CultureInfo.InvariantCulture));
        }
    }
}
