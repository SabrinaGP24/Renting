using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WPF_Hexagonal_DDD.Views;

namespace WPF_Hexagonal_DDD
{
    public partial class App : Application
    {
        private IHost _host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                _host = Bootstrapper.BuildHost();
                await _host.StartAsync();

                MainWindow window = _host.Services.GetRequiredService<MainWindow>();
                MainWindow = window;
                window.Show();
            }
            catch (Exception exception)
            {
                // Tipicamente: proveedor de persistencia mal configurado o base de datos inaccesible.
                MessageBox.Show(
                    "No se pudo iniciar la aplicacion." + Environment.NewLine + Environment.NewLine + exception.Message,
                    "Error de arranque",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown(-1);
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
                _host = null;
            }

            base.OnExit(e);
        }
    }
}
