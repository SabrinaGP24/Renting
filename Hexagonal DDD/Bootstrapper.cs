using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HexagonalDDD.Applicaion.DependencyInjection;
using HexagonalDDD.Infraestructure.Configuration;
using HexagonalDDD.Infraestructure.DependencyInjection;
using WPF_Hexagonal_DDD.ViewModels;
using WPF_Hexagonal_DDD.Views;

namespace WPF_Hexagonal_DDD
{
    /// <summary>
    /// Composition root: el unico punto donde se ensamblan las capas. Se expone como metodo publico
    /// para que los tests de infraestructura puedan montar exactamente el mismo contenedor que la
    /// aplicacion real, sin arrancar la interfaz grafica.
    /// </summary>
    public static class Bootstrapper
    {
        public static IHost BuildHost()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    // El proveedor de persistencia (InMemory / Oracle / SqlServer) se lee del App.config.
                    services.AddInfrastructure(PersistenceOptions.FromAppConfig());
                    services.AddApplication();

                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MainWindow>();

                    // La ventana de alquiler es transitoria: una instancia nueva por cada alquiler.
                    // Se inyecta como factoria porque quien la abre es la vista, no el contenedor.
                    services.AddTransient<RentVehicleViewModel>();
                    services.AddTransient<RentVehicleWindow>();
                    services.AddSingleton<Func<RentVehicleWindow>>(
                        provider => () => provider.GetRequiredService<RentVehicleWindow>());
                })
                .Build();
        }
    }
}
