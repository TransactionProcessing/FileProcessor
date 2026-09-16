using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace FileProcessor.BusinessLogic.Tests;

internal sealed class CapturingLogger<T> : ILogger<T>
{
    public List<string> Messages { get; } = new();

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel,
                            EventId eventId,
                            TState state,
                            Exception exception,
                            Func<TState, Exception, string> formatter)
    {
        this.Messages.Add(formatter(state, exception));
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
