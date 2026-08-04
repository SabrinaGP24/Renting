using System;

namespace HexagonalDDD.Domain.Services
{
    /// <summary>
    /// Puerto de salida para el reloj: evita depender de DateTime.Now en dominio y aplicacion,
    /// de forma que los tests puedan fijar la fecha actual.
    /// </summary>
    public interface IClock
    {
        DateTime Now { get; }
    }
}
