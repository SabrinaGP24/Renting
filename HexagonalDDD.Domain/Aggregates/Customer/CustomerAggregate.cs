using System;
using HexagonalDDD.Domain.Exceptions;

namespace HexagonalDDD.Domain.Aggregates.Customer
{
    /// <summary>
    /// Cliente de la empresa de renting. Su identidad es el documento identificativo (DNI/NIE),
    /// que es clave natural: por eso no lleva un Guid artificial y RENTALS puede referenciarlo
    /// directamente con una clave ajena.
    /// </summary>
    public class CustomerAggregate
    {
        private CustomerAggregate(
            string documentId,
            string firstName,
            string lastName,
            string phone,
            DateTime registeredAt)
        {
            DocumentId = documentId;
            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
            RegisteredAt = registeredAt;
        }

        public string DocumentId { get; private set; }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public string Phone { get; private set; }

        public DateTime RegisteredAt { get; private set; }

        public string FullName
        {
            get { return (FirstName + " " + LastName).Trim(); }
        }

        public static CustomerAggregate Register(
            string documentId,
            string firstName,
            string lastName,
            string phone,
            DateTime registeredAt)
        {
            string normalizedDocument = NormalizeDocumentId(documentId);
            if (normalizedDocument.Length == 0)
            {
                throw new DomainValidationException("El documento identificativo del cliente es obligatorio.");
            }

            return new CustomerAggregate(
                normalizedDocument,
                RequireName(firstName, "El nombre del cliente es obligatorio."),
                RequireName(lastName, "Los apellidos del cliente son obligatorios."),
                Normalize(phone),
                registeredAt);
        }

        public static CustomerAggregate Rehydrate(
            string documentId,
            string firstName,
            string lastName,
            string phone,
            DateTime registeredAt)
        {
            return new CustomerAggregate(documentId, firstName, lastName, phone, registeredAt);
        }

        /// <summary>
        /// Permite corregir los datos de contacto en un alquiler posterior sin dar de alta
        /// un cliente nuevo: el documento identificativo no cambia nunca.
        /// </summary>
        public void UpdateContactData(string firstName, string lastName, string phone)
        {
            FirstName = RequireName(firstName, "El nombre del cliente es obligatorio.");
            LastName = RequireName(lastName, "Los apellidos del cliente son obligatorios.");
            Phone = Normalize(phone);
        }

        /// <summary>
        /// Normalizacion unica del documento (recortado y en mayusculas). La usan tambien la capa
        /// de aplicacion y los repositorios, para que '12345678z' y '12345678Z' sean el mismo cliente
        /// y la regla de un alquiler por persona no se pueda esquivar cambiando mayusculas.
        /// </summary>
        public static string NormalizeDocumentId(string documentId)
        {
            return documentId == null ? string.Empty : documentId.Trim().ToUpperInvariant();
        }

        private static string RequireName(string value, string errorMessage)
        {
            string normalized = Normalize(value);
            if (normalized.Length == 0)
            {
                throw new DomainValidationException(errorMessage);
            }

            return normalized;
        }

        private static string Normalize(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }
}
