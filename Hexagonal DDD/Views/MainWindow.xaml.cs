using System;
using System.Windows;
using HexagonalDDD.Applicaion.DTOs;
using WPF_Hexagonal_DDD.ViewModels;

namespace WPF_Hexagonal_DDD.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly Func<RentVehicleWindow> _rentVehicleWindowFactory;

        public MainWindow(MainViewModel viewModel, Func<RentVehicleWindow> rentVehicleWindowFactory)
        {
            InitializeComponent();

            _viewModel = viewModel;
            _rentVehicleWindowFactory = rentVehicleWindowFactory;
            DataContext = viewModel;

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            await _viewModel.RefreshAsync();
        }

        /// <summary>
        /// Abrir una ventana es responsabilidad de la vista, no del ViewModel: por eso el dialogo
        /// se lanza desde el code-behind y no desde un comando.
        /// </summary>
        private async void OnRentClick(object sender, RoutedEventArgs e)
        {
            VehicleDto vehicle = _viewModel.SelectedVehicle;
            if (vehicle == null)
            {
                return;
            }

            RentVehicleWindow window = _rentVehicleWindowFactory();
            window.Owner = this;
            window.Initialize(vehicle);

            if (window.ShowDialog() == true)
            {
                await _viewModel.OnRentCompletedAsync(window.ResultMessage);
            }
        }
    }
}
