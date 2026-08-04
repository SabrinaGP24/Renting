using System;
using HexagonalDDD.Domain.Services;

namespace HexagonalDDD.Unit.Test.Fakes
{
    /// <summary>
    /// Reloj con fecha fija: sin el, los tests de la regla de antiguedad dejarian de pasar
    /// con el paso del tiempo.
    /// </summary>
    internal sealed class FixedClock : IClock
    {
        private readonly DateTime _now;

        public FixedClock(DateTime now)
        {
            _now = now;
        }

        public DateTime Now
        {
            get { return _now; }
        }
    }
}
