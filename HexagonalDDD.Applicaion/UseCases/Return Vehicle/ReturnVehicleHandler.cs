using System;
using System.Threading.Tasks;
using HexagonalDDD.Applicaion.DTOs;
using HexagonalDDD.Domain.Aggregates.Rental;
using HexagonalDDD.Domain.Aggregates.Vehicle;
using HexagonalDDD.Domain.Exceptions;
using HexagonalDDD.Domain.Repositories;
using HexagonalDDD.Domain.Services;

namespace HexagonalDDD.Applicaion.UseCases.Return_Vehicle
{
    /// <summary>
    /// Devolucion de un vehiculo: cierra el alquiler activo y devuelve el vehiculo a la flota.
    /// </summary>
    public class ReturnVehicleHandler
    {
        private readonly IVehicleRepository _vehicles;
        private readonly IRentalRepository _rentals;
        private readonly IClock _clock;

        public ReturnVehicleHandler(IVehicleRepository vehicles, IRentalRepository rentals, IClock clock)
        {
            if (vehicles == null)
            {
                throw new ArgumentNullException("vehicles");
            }

            if (rentals == null)
            {
                throw new ArgumentNullException("rentals");
            }

            if (clock == null)
            {
                throw new ArgumentNullException("clock");
            }

            _vehicles = vehicles;
            _rentals = rentals;
            _clock = clock;
        }

        public async Task<RentalDto> ExecuteAsync(ReturnVehicleCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            VehicleAggregate vehicle = await _vehicles.GetByIdAsync(command.VehicleId);
            if (vehicle == null)
            {
                throw new VehicleNotFoundException(command.VehicleId);
            }

            RentalAggregate rental = await _rentals.GetActiveByVehicleIdAsync(vehicle.Id);
            if (rental == null)
            {
                throw new ActiveRentalNotFoundException(vehicle.Id);
            }

            // Lanza VehicleNotRentedException si el vehiculo no estaba alquilado.
            vehicle.Return();
            rental.Close(_clock.Now);

            await _rentals.UpdateAsync(rental);
            await _vehicles.UpdateAsync(vehicle);

            return RentalDto.From(rental);
        }
    }
}
