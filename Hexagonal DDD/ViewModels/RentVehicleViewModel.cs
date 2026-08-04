using System;
using System.Threading.Tasks;
using System.Windows.Input;
using HexagonalDDD.Applicaion.DTOs;
using HexagonalDDD.Applicaion.UseCases.Find_Customer;
using HexagonalDDD.Applicaion.UseCases.Rent_Vehicle;
using HexagonalDDD.Domain.Exceptions;

namespace WPF_Hexagonal_DDD.ViewModels
{
    /// <summary>
    /// ViewModel de la ventana de alquiler: se introduce el documento del cliente, se buscan sus
    /// datos y, si no existe, se dan de alta en el mismo acto de alquilar.
    /// </summary>
    public class RentVehicleViewModel : ViewModelBase
    {
        private readonly FindCustomerHandler _findCustomer;
        private readonly RentVehicleHandler _rentVehicle;

        private VehicleDto _vehicle;
        private string _documentId;
        private string _firstName;
        private string _lastName;
        private string _phone;
        private string _statusMessage;
        private bool _hasError;
        private bool _customerLookedUp;
        private bool _isKnownCustomer;

        public RentVehicleViewModel(FindCustomerHandler findCustomer, RentVehicleHandler rentVehicle)
        {
            if (findCustomer == null)
            {
                throw new ArgumentNullException("findCustomer");
            }

            if (rentVehicle == null)
            {
                throw new ArgumentNullException("rentVehicle");
            }

            _findCustomer = findCustomer;
            _rentVehicle = rentVehicle;

            SearchCustomerCommand = new AsyncRelayCommand(SearchCustomerAsync, CanSearchCustomer);
            ConfirmCommand = new AsyncRelayCommand(ConfirmAsync, CanConfirm);
        }

        /// <summary>Se dispara cuando el alquiler se ha registrado; la ventana se cierra al recibirlo.</summary>
        public event EventHandler<string> Completed;

        public ICommand SearchCustomerCommand { get; private set; }

        public ICommand ConfirmCommand { get; private set; }

        public string VehicleDescription
        {
            get
            {
                return _vehicle == null
                    ? string.Empty
                    : _vehicle.PlateNumber + "  -  " + _vehicle.Brand + " " + _vehicle.Model;
            }
        }

        public string DocumentId
        {
            get { return _documentId; }
            set
            {
                if (SetProperty(ref _documentId, value))
                {
                    // Cambiar el documento invalida la busqueda anterior.
                    CustomerLookedUp = false;
                    IsKnownCustomer = false;
                }
            }
        }

        public string FirstName
        {
            get { return _firstName; }
            set { SetProperty(ref _firstName, value); }
        }

        public string LastName
        {
            get { return _lastName; }
            set { SetProperty(ref _lastName, value); }
        }

        public string Phone
        {
            get { return _phone; }
            set { SetProperty(ref _phone, value); }
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

        /// <summary>Habilita el formulario de datos personales solo despues de buscar el documento.</summary>
        public bool CustomerLookedUp
        {
            get { return _customerLookedUp; }
            private set { SetProperty(ref _customerLookedUp, value); }
        }

        public bool IsKnownCustomer
        {
            get { return _isKnownCustomer; }
            private set { SetProperty(ref _isKnownCustomer, value); }
        }

        public void Initialize(VehicleDto vehicle)
        {
            _vehicle = vehicle;
            OnPropertyChanged("VehicleDescription");
            StatusMessage = "Introduce el documento del cliente y pulsa Buscar.";
            HasError = false;
        }

        private bool CanSearchCustomer()
        {
            return !string.IsNullOrWhiteSpace(_documentId);
        }

        private bool CanConfirm()
        {
            return _vehicle != null
                && !string.IsNullOrWhiteSpace(_documentId)
                && !string.IsNullOrWhiteSpace(_firstName)
                && !string.IsNullOrWhiteSpace(_lastName);
        }

        private async Task SearchCustomerAsync()
        {
            await ExecuteAsync(async () =>
            {
                CustomerDto customer = await _findCustomer.ExecuteAsync(
                    new FindCustomerQuery { DocumentId = _documentId });

                CustomerLookedUp = true;

                if (customer == null)
                {
                    IsKnownCustomer = false;
                    FirstName = null;
                    LastName = null;
                    Phone = null;
                    Report("Cliente nuevo: rellena sus datos para darlo de alta.", false);
                    return;
                }

                IsKnownCustomer = true;
                FirstName = customer.FirstName;
                LastName = customer.LastName;
                Phone = customer.Phone;
                Report("Cliente encontrado: " + customer.FirstName + " " + customer.LastName + ".", false);
            });
        }

        private async Task ConfirmAsync()
        {
            await ExecuteAsync(async () =>
            {
                RentalDto rental = await _rentVehicle.ExecuteAsync(new RentVehicleCommand
                {
                    VehicleId = _vehicle.Id,
                    CustomerDocumentId = _documentId,
                    FirstName = _firstName,
                    LastName = _lastName,
                    Phone = _phone
                });

                EventHandler<string> handler = Completed;
                if (handler != null)
                {
                    handler(this, "Vehiculo " + _vehicle.PlateNumber + " alquilado a " +
                                  rental.CustomerDocumentId + ".");
                }
            });
        }

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

        private void Report(string message, bool isError)
        {
            StatusMessage = message;
            HasError = isError;
        }
    }
}
