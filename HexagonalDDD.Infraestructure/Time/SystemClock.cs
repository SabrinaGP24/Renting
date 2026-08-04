using System;
using HexagonalDDD.Domain.Services;

namespace HexagonalDDD.Infraestructure.Time
{
    public class SystemClock : IClock
    {
        public DateTime Now
        {
            get { return DateTime.Now; }
        }
    }
}
