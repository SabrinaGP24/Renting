using System;
using Microsoft.Extensions.DependencyInjection;
using HexagonalDDD.Domain.Repositories;
using HexagonalDDD.Domain.Services;
using HexagonalDDD.Infraestructure.Configuration;
using HexagonalDDD.Infraestructure.Persistence.InMemory;
using HexagonalDDD.Infraestructure.Persistence.Oracle;
using HexagonalDDD.Infraestructure.Persistence.SqlServer;
using HexagonalDDD.Infraestructure.Time;

namespace HexagonalDDD.Infraestructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Enchufa los adaptadores de salida a los puertos del dominio segun el proveedor configurado.
        /// Es el unico punto de la solucion que conoce que implementacion concreta se usa.
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, PersistenceOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            services.AddSingleton<IClock, SystemClock>();

            switch (options.Provider)
            {
                case PersistenceProvider.InMemory:
                    // Singleton: el almacen vive en el proceso y debe sobrevivir entre casos de uso.
                    services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
                    services.AddSingleton<IRentalRepository, InMemoryRentalRepository>();
                    services.AddSingleton<ICustomerRepository, InMemoryCustomerRepository>();
                    break;

                case PersistenceProvider.Oracle:
                    services.AddTransient<IVehicleRepository>(
                        provider => new OracleVehicleRepository(options.ConnectionString));
                    services.AddTransient<IRentalRepository>(
                        provider => new OracleRentalRepository(options.ConnectionString));
                    services.AddTransient<ICustomerRepository>(
                        provider => new OracleCustomerRepository(options.ConnectionString));
                    break;

                case PersistenceProvider.SqlServer:
                    services.AddTransient<IVehicleRepository>(
                        provider => new SqlServerVehicleRepository(options.ConnectionString));
                    services.AddTransient<IRentalRepository>(
                        provider => new SqlServerRentalRepository(options.ConnectionString));
                    services.AddTransient<ICustomerRepository>(
                        provider => new SqlServerCustomerRepository(options.ConnectionString));
                    break;

                default:
                    throw new NotSupportedException("Proveedor de persistencia no soportado: " + options.Provider);
            }

            return services;
        }
    }
}
