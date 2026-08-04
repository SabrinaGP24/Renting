using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HexagonalDDD.Applicaion.DTOs;
using HexagonalDDD.Domain.Aggregates.Vehicle;
using HexagonalDDD.Domain.Repositories;

namespace HexagonalDDD.Applicaion.UseCases.List_Vehicles
{
    public class ListAvailableVehiclesHandler
    {
        private readonly IVehicleRepository _vehicles;

        public ListAvailableVehiclesHandler(IVehicleRepository vehicles)
        {
            if (vehicles == null)
            {
                throw new ArgumentNullException("vehicles");
            }

            _vehicles = vehicles;
        }

        public async Task<IReadOnlyCollection<VehicleDto>> ExecuteAsync(ListAvailableVehiclesQuery query)
        {
            IReadOnlyCollection<VehicleAggregate> vehicles = await _vehicles.GetAvailableAsync();

            return vehicles
                .OrderBy(v => v.PlateNumber)
                .Select(VehicleDto.From)
                .ToList();
        }
    }
}
