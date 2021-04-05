using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace RESTfulWebInterface.Tests
{
    public class MockLogger<T> : ILogger<T>, IDisposable
    {
        public IDisposable BeginScope<TState>(TState state) => this;

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
        }

        public void Dispose() { }
    }
}
