using System;
using System.Threading.Tasks;
using HexagonalDDD.Applicaion.DTOs;
using HexagonalDDD.Domain.Aggregates.Customer;
using HexagonalDDD.Domain.Aggregates.Rental;
using HexagonalDDD.Domain.Aggregates.Vehicle;
using HexagonalDDD.Domain.Exceptions;
using HexagonalDDD.Domain.Repositories;
using HexagonalDDD.Domain.Services;

namespace HexagonalDDD.Applicaion.UseCases.Rent_Vehicle
{
    /// <summary>
    /// Alquiler de un vehiculo.
    /// La regla "una persona no puede tener mas de un vehiculo alquilado a la vez" se resuelve aqui
    /// y no en el agregado, porque abarca el conjunto de alquileres del cliente y necesita el repositorio.
    /// Si el cliente no esta dado de alta, se registra en el mismo acto.
    /// </summary>
    public class RentVehicleHandler
    {
        private readonly IVehicleRepository _vehicles;
        private readonly IRentalRepository _rentals;
        private readonly ICustomerRepository _customers;
        private readonly IClock _clock;

        public RentVehicleHandler(
            IVehicleRepository vehicles,
            IRentalRepository rentals,
            ICustomerRepository customers,
            IClock clock)
        {
            if (vehicles == null)
            {
                throw new ArgumentNullException("vehicles");
            }

            if (rentals == null)
            {
                throw new ArgumentNullException("rentals");
            }

            if (customers == null)
            {
                throw new ArgumentNullException("customers");
            }

            if (clock == null)
            {
                throw new ArgumentNullException("clock");
            }

            _vehicles = vehicles;
            _rentals = rentals;
            _customers = customers;
            _clock = clock;
        }

        public async Task<RentalDto> ExecuteAsync(RentVehicleCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            string customerDocumentId = CustomerAggregate.NormalizeDocumentId(command.CustomerDocumentId);
            if (customerDocumentId.Length == 0)
            {
                throw new DomainValidationException("El documento identificativo del cliente es obligatorio.");
            }

            VehicleAggregate vehicle = await _vehicles.GetByIdAsync(command.VehicleId);
            if (vehicle == null)
            {
                throw new VehicleNotFoundException(command.VehicleId);
            }

            if (await _rentals.HasActiveRentalAsync(customerDocumentId))
            {
                throw new CustomerAlreadyHasActiveRentalException(customerDocumentId);
            }

            // El cliente debe existir antes de insertar el alquiler: RENTALS tiene una clave ajena
            // contra CUSTOMERS.
            await EnsureCustomerAsync(customerDocumentId, command);

            // Lanza VehicleNotAvailableException si el vehiculo ya estaba alquilado.
            vehicle.Rent();

            RentalAggregate rental = RentalAggregate.Start(
                Guid.NewGuid(),
                vehicle.Id,
                customerDocumentId,
                _clock.Now);

            await _rentals.AddAsync(rental);
            await _vehicles.UpdateAsync(vehicle);

            return RentalDto.From(rental);
        }

        private async Task EnsureCustomerAsync(string customerDocumentId, RentVehicleCommand command)
        {
            CustomerAggregate customer = await _customers.GetByDocumentIdAsync(customerDocumentId);

            if (customer == null)
            {
                customer = CustomerAggregate.Register(
                    customerDocumentId,
                    command.FirstName,
                    command.LastName,
                    command.Phone,
                    _clock.Now);

                await _customers.AddAsync(customer);
                return;
            }

            customer.UpdateContactData(command.FirstName, command.LastName, command.Phone);
            await _customers.UpdateAsync(customer);
        }
    }
}
