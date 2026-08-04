using Microsoft.Extensions.DependencyInjection;
using HexagonalDDD.Applicaion.UseCases.Create_Vehicle;
using HexagonalDDD.Applicaion.UseCases.Find_Customer;
using HexagonalDDD.Applicaion.UseCases.List_Vehicles;
using HexagonalDDD.Applicaion.UseCases.Rent_Vehicle;
using HexagonalDDD.Applicaion.UseCases.Return_Vehicle;

namespace HexagonalDDD.Applicaion.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddTransient<CreateVehicleHandler>();
            services.AddTransient<ListAvailableVehiclesHandler>();
            services.AddTransient<ListAllVehiclesHandler>();
            services.AddTransient<FindCustomerHandler>();
            services.AddTransient<RentVehicleHandler>();
            services.AddTransient<ReturnVehicleHandler>();
            return services;
        }
    }
}
