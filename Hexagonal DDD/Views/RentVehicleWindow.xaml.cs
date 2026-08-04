using System.Windows;
using HexagonalDDD.Applicaion.DTOs;
using WPF_Hexagonal_DDD.ViewModels;

namespace WPF_Hexagonal_DDD.Views
{
    public partial class RentVehicleWindow : Window
    {
        private readonly RentVehicleViewModel _viewModel;

        public RentVehicleWindow(RentVehicleViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = viewModel;

            _viewModel.Completed += OnCompleted;
        }

        /// <summary>Mensaje que la ventana principal muestra cuando el alquiler se ha registrado.</summary>
        public string ResultMessage { get; private set; }

        public void Initialize(VehicleDto vehicle)
        {
            _viewModel.Initialize(vehicle);
        }

        private void OnCompleted(object sender, string message)
        {
            _viewModel.Completed -= OnCompleted;

            ResultMessage = message;
            DialogResult = true;
        }
    }
}
