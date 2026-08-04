using System;
using System.Globalization;
using System.Threading.Tasks;
using HexagonalDDD.Applicaion.DTOs;
using HexagonalDDD.Domain.Aggregates.Vehicle;
using HexagonalDDD.Domain.Exceptions;
using HexagonalDDD.Domain.Repositories;
using HexagonalDDD.Domain.Services;

namespace HexagonalDDD.Applicaion.UseCases.Create_Vehicle
{
    /// <summary>
    /// Alta de un vehiculo en la flota. La regla de antiguedad maxima vive en el agregado;
    /// aqui solo se comprueba la unicidad de matricula, que exige consultar el repositorio.
    /// </summary>
    public class CreateVehicleHandler
    {
        private readonly IVehicleRepository _vehicles;
        private readonly IClock _clock;

        public CreateVehicleHandler(IVehicleRepository vehicles, IClock clock)
        {
            if (vehicles == null)
            {
                throw new ArgumentNullException("vehicles");
            }

            if (clock == null)
            {
                throw new ArgumentNullException("clock");
            }

            _vehicles = vehicles;
            _clock = clock;
        }

        public async Task<VehicleDto> ExecuteAsync(CreateVehicleCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            VehicleAggregate vehicle = VehicleAggregate.Create(
                Guid.NewGuid(),
                command.PlateNumber,
                command.Brand,
                command.Model,
                command.ManufactureDate,
                _clock.Now);

            VehicleAggregate existing = await _vehicles.GetByPlateNumberAsync(vehicle.PlateNumber);
            if (existing != null)
            {
                throw new DomainValidationException(string.Format(
                    CultureInfo.CurrentCulture,
                    "Ya existe un vehiculo en la flota con la matricula '{0}'.",
                    vehicle.PlateNumber));
            }

            await _vehicles.AddAsync(vehicle);

            return VehicleDto.From(vehicle);
        }
    }
}
