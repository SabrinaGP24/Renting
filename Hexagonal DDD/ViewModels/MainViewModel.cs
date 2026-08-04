using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using HexagonalDDD.Applicaion.DTOs;
using HexagonalDDD.Applicaion.UseCases.Create_Vehicle;
using HexagonalDDD.Applicaion.UseCases.List_Vehicles;
using HexagonalDDD.Applicaion.UseCases.Return_Vehicle;
using HexagonalDDD.Domain.Exceptions;

namespace WPF_Hexagonal_DDD.ViewModels
{
    /// <summary>
    /// Unico adaptador de entrada de la aplicacion: traduce gestos del usuario a comandos/consultas
    /// de la capa de aplicacion. No contiene ninguna regla de negocio.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly CreateVehicleHandler _createVehicle;
        private readonly ListAllVehiclesHandler _listAllVehicles;
        private readonly ListAvailableVehiclesHandler _listAvailableVehicles;
        private readonly ReturnVehicleHandler _returnVehicle;

        private VehicleDto _selectedVehicle;
        private bool _showOnlyAvailable;
        private string _newPlateNumber;
        private string _newBrand;
        private string _newModel;
        private DateTime? _newManufactureDate;
        private string _statusMessage;
        private bool _hasError;

        public MainViewModel(
            CreateVehicleHandler createVehicle,
            ListAllVehiclesHandler listAllVehicles,
            ListAvailableVehiclesHandler listAvailableVehicles,
            ReturnVehicleHandler returnVehicle)
        {
            if (createVehicle == null)
            {
                throw new ArgumentNullException("createVehicle");
            }

            if (listAllVehicles == null)
            {
                throw new ArgumentNullException("listAllVehicles");
            }

            if (listAvailableVehicles == null)
            {
                throw new ArgumentNullException("listAvailableVehicles");
            }

            if (returnVehicle == null)
            {
                throw new ArgumentNullException("returnVehicle");
            }

            _createVehicle = createVehicle;
            _listAllVehicles = listAllVehicles;
            _listAvailableVehicles = listAvailableVehicles;
            _returnVehicle = returnVehicle;
            _newManufactureDate = DateTime.Today;

            Vehicles = new ObservableCollection<VehicleDto>();

            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            AddVehicleCommand = new AsyncRelayCommand(AddVehicleAsync);
            ReturnCommand = new AsyncRelayCommand(ReturnAsync, CanReturnSelectedVehicle);
        }

        public ObservableCollection<VehicleDto> Vehicles { get; private set; }

        public ICommand RefreshCommand { get; private set; }

        public ICommand AddVehicleCommand { get; private set; }

        public ICommand ReturnCommand { get; private set; }

        public VehicleDto SelectedVehicle
        {
            get { return _selectedVehicle; }
            set
            {
                if (SetProperty(ref _selectedVehicle, value))
                {
                    OnPropertyChanged("CanRentSelectedVehicle");
                }
            }
        }

        /// <summary>Habilita el boton que abre la ventana de alquiler.</summary>
        public bool CanRentSelectedVehicle
        {
            get { return _selectedVehicle != null && _selectedVehicle.IsAvailable; }
        }

        public bool ShowOnlyAvailable
        {
            get { return _showOnlyAvailable; }
            set
            {
                if (SetProperty(ref _showOnlyAvailable, value))
                {
                    RefreshCommand.Execute(null);
                }
            }
        }

        public string NewPlateNumber
        {
            get { return _newPlateNumber; }
            set { SetProperty(ref _newPlateNumber, value); }
        }

        public string NewBrand
        {
            get { return _newBrand; }
            set { SetProperty(ref _newBrand, value); }
        }

        public string NewModel
        {
            get { return _newModel; }
            set { SetProperty(ref _newModel, value); }
        }

        public DateTime? NewManufactureDate
        {
            get { return _newManufactureDate; }
            set { SetProperty(ref _newManufactureDate, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            private set { SetProperty(ref _statusMessage, value); }
        }

        public bool HasError
        {
            get { return _hasError; }
            private set { SetProperty(ref _hasError, value); }
        }

        public async Task RefreshAsync()
        {
            await ExecuteAsync(async () =>
            {
                Guid? previousSelection = _selectedVehicle == null ? (Guid?)null : _selectedVehicle.Id;

                await LoadVehiclesAsync();

                SelectedVehicle = previousSelection.HasValue ? FindVehicle(previousSelection.Value) : null;
            });
        }

        private async Task AddVehicleAsync()
        {
            await ExecuteAsync(async () =>
            {
                if (!_newManufactureDate.HasValue)
                {
                    throw new DomainValidationException("Indica la fecha de fabricacion del vehiculo.");
                }

                CreateVehicleCommand command = new CreateVehicleCommand
                {
                    PlateNumber = _newPlateNumber,
                    Brand = _newBrand,
                    Model = _newModel,
                    ManufactureDate = _newManufactureDate.Value
                };

                VehicleDto created = await _createVehicle.ExecuteAsync(command);

                NewPlateNumber = null;
                NewBrand = null;
                NewModel = null;
                NewManufactureDate = DateTime.Today;

                await LoadVehiclesAsync();
                SelectedVehicle = FindVehicle(created.Id);

                Report("Vehiculo " + created.PlateNumber + " dado de alta en la flota.", false);
            });
        }

        /// <summary>
        /// La ventana de alquiler ya ha registrado la operacion; aqui solo se refresca la flota
        /// y se traslada el mensaje a la barra de estado.
        /// </summary>
        public async Task OnRentCompletedAsync(string message)
        {
            await ExecuteAsync(async () =>
            {
                Guid? previousSelection = _selectedVehicle == null ? (Guid?)null : _selectedVehicle.Id;

                await LoadVehiclesAsync();
                SelectedVehicle = previousSelection.HasValue ? FindVehicle(previousSelection.Value) : null;

                Report(message, false);
            });
        }

        private async Task ReturnAsync()
        {
            await ExecuteAsync(async () =>
            {
                VehicleDto vehicle = _selectedVehicle;

                ReturnVehicleCommand command = new ReturnVehicleCommand { VehicleId = vehicle.Id };

                RentalDto rental = await _returnVehicle.ExecuteAsync(command);

                await LoadVehiclesAsync();
                SelectedVehicle = FindVehicle(vehicle.Id);

                Report("Vehiculo " + vehicle.PlateNumber + " devuelto por " + rental.CustomerDocumentId + ".", false);
            });
        }

        private bool CanReturnSelectedVehicle()
        {
            return _selectedVehicle != null && !_selectedVehicle.IsAvailable;
        }

        private async Task LoadVehiclesAsync()
        {
            IReadOnlyCollection<VehicleDto> vehicles = _showOnlyAvailable
                ? await _listAvailableVehicles.ExecuteAsync(new ListAvailableVehiclesQuery())
                : await _listAllVehicles.ExecuteAsync(new ListAllVehiclesQuery());

            Vehicles.Clear();
            foreach (VehicleDto vehicle in vehicles)
            {
                Vehicles.Add(vehicle);
            }
        }

        /// <summary>
        /// Convierte las excepciones de negocio en mensajes para el usuario y deja pasar el resto
        /// como fallo tecnico, sin tumbar la aplicacion.
        /// </summary>
        private async Task ExecuteAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (DomainException exception)
            {
                Report(exception.Message, true);
            }
            catch (Exception exception)
            {
                Report("Error tecnico: " + exception.Message, true);
            }
        }

        private VehicleDto FindVehicle(Guid id)
        {
            foreach (VehicleDto vehicle in Vehicles)
            {
                if (vehicle.Id == id)
                {
                    return vehicle;
                }
            }

            return null;
        }

        private void Report(string message, bool isError)
        {
            StatusMessage = message;
            HasError = isError;
        }
    }
}
