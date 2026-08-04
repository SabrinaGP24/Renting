using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using HexagonalDDD.Domain.Aggregates.Vehicle;
using HexagonalDDD.Domain.Repositories;
using Oracle.ManagedDataAccess.Client;

namespace HexagonalDDD.Infraestructure.Persistence.Oracle
{
    /// <summary>
    /// Adaptador Oracle con ADO.NET puro sobre un esquema database-first (tabla VEHICLES).
    /// Los Guid se almacenan como RAW(16).
    /// </summary>
    public class OracleVehicleRepository : IVehicleRepository
    {
        private const string SelectColumns = "ID, PLATE_NUMBER, BRAND, MODEL, MANUFACTURE_DATE, STATUS";

        private readonly string _connectionString;

        public OracleVehicleRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("La cadena de conexion de Oracle es obligatoria.", "connectionString");
            }

            _connectionString = connectionString;
        }

        public async Task<VehicleAggregate> GetByIdAsync(Guid id)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + SelectColumns + " FROM VEHICLES WHERE ID = :p_id";
                command.BindByName = true;
                command.Parameters.Add("p_id", OracleDbType.Raw).Value = id.ToByteArray();

                await connection.OpenAsync();
                return await ReadSingleAsync(command);
            }
        }

        public async Task<VehicleAggregate> GetByPlateNumberAsync(string plateNumber)
        {
            string normalized = plateNumber == null ? string.Empty : plateNumber.Trim().ToUpperInvariant();

            using (OracleConnection connection = new OracleConnection(_connectionString))
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + SelectColumns + " FROM VEHICLES WHERE PLATE_NUMBER = :p_plate";
                command.BindByName = true;
                command.Parameters.Add("p_plate", OracleDbType.Varchar2).Value = normalized;

                await connection.OpenAsync();
                return await ReadSingleAsync(command);
            }
        }

        public async Task<IReadOnlyCollection<VehicleAggregate>> GetAllAsync()
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT " + SelectColumns + " FROM VEHICLES ORDER BY PLATE_NUMBER";
                command.BindByName = true;

                await connection.OpenAsync();
                return await ReadManyAsync(command);
            }
        }

        public async Task<IReadOnlyCollection<VehicleAggregate>> GetAvailableAsync()
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT " + SelectColumns + " FROM VEHICLES WHERE STATUS = :p_status ORDER BY PLATE_NUMBER";
                command.BindByName = true;
                command.Parameters.Add("p_status", OracleDbType.Int32).Value = (int)VehicleStatus.Available;

                await connection.OpenAsync();
                return await ReadManyAsync(command);
            }
        }

        public async Task AddAsync(VehicleAggregate vehicle)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "INSERT INTO VEHICLES (ID, PLATE_NUMBER, BRAND, MODEL, MANUFACTURE_DATE, STATUS) " +
                    "VALUES (:p_id, :p_plate, :p_brand, :p_model, :p_manufacture_date, :p_status)";
                command.BindByName = true;
                AddVehicleParameters(command, vehicle);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateAsync(VehicleAggregate vehicle)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            using (OracleCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    "UPDATE VEHICLES SET PLATE_NUMBER = :p_plate, BRAND = :p_brand, MODEL = :p_model, " +
                    "MANUFACTURE_DATE = :p_manufacture_date, STATUS = :p_status WHERE ID = :p_id";
                command.BindByName = true;
                AddVehicleParameters(command, vehicle);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        private static void AddVehicleParameters(OracleCommand command, VehicleAggregate vehicle)
        {
            command.Parameters.Add("p_id", OracleDbType.Raw).Value = vehicle.Id.ToByteArray();
            command.Parameters.Add("p_plate", OracleDbType.Varchar2).Value = vehicle.PlateNumber;
            command.Parameters.Add("p_brand", OracleDbType.Varchar2).Value = vehicle.Brand;
            command.Parameters.Add("p_model", OracleDbType.Varchar2).Value = vehicle.Model;
            command.Parameters.Add("p_manufacture_date", OracleDbType.Date).Value = vehicle.ManufactureDate;
            command.Parameters.Add("p_status", OracleDbType.Int32).Value = (int)vehicle.Status;
        }

        private static async Task<VehicleAggregate> ReadSingleAsync(OracleCommand command)
        {
            using (IDataReader reader = await command.ExecuteReaderAsync())
            {
                return reader.Read() ? Map(reader) : null;
            }
        }

        private static async Task<IReadOnlyCollection<VehicleAggregate>> ReadManyAsync(OracleCommand command)
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
                new Guid((byte[])reader.GetValue(0)),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetDateTime(4),
                (VehicleStatus)Convert.ToInt32(reader.GetValue(5), CultureInfo.InvariantCulture));
        }
    }
}
