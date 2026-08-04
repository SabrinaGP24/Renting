using System;

namespace HexagonalDDD.Domain.Exceptions
{
    /// <summary>
    /// Raiz de todas las excepciones de negocio. La capa de presentacion la captura
    /// para distinguir un error de reglas de negocio de un fallo tecnico.
    /// </summary>
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }
    }
}
