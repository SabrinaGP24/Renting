using System.Globalization;

namespace HexagonalDDD.Domain.Exceptions
{
    public class CustomerAlreadyHasActiveRentalException : DomainException
    {
        public string CustomerDocumentId { get; private set; }

        public CustomerAlreadyHasActiveRentalException(string customerDocumentId)
            : base(string.Format(
                CultureInfo.CurrentCulture,
                "El cliente '{0}' ya tiene un vehiculo alquilado. Debe devolverlo antes de alquilar otro.",
                customerDocumentId))
        {
            CustomerDocumentId = customerDocumentId;
        }
    }
}
