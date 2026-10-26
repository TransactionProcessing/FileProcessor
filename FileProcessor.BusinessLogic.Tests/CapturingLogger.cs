using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Logger;

namespace FileProcessor.BusinessLogic.Tests;

internal sealed class CapturingLogger : Shared.Logger.ILogger
{
    public bool IsInitialised { get; set; } = true;

    public List<string> Messages { get; } = new();

    public void LogCritical(string message) => this.Capture(message);
    public void LogCritical(Exception exception) => this.Capture(exception.ToString());
    public void LogCritical(string message, params object[] args) => this.Capture(message, args);
    public void LogCritical(Exception exception, string message, params object[] args) => this.Capture(message, args, exception.ToString());
    public void LogCritical(string message, Exception exception) => this.Capture(message, null, exception.ToString());
    public void LogDebug(string message) => this.Capture(message);
    public void LogDebug(string message, params object[] args) => this.Capture(message, args);
    public void LogError(Exception exception) => this.Capture(exception.ToString());
    public void LogError(string message, params object[] args) => this.Capture(message, args);
    public void LogError(Exception exception, string message, params object[] args) => this.Capture(message, args, exception.ToString());
    public void LogError(string message) => this.Capture(message);
    public void LogError(string message, Exception exception) => this.Capture(message, null, exception.ToString());
    public void LogInformation(string message) => this.Capture(message);
    public void LogInformation(string message, params object[] args) => this.Capture(message, args);
    public void LogTrace(string message) => this.Capture(message);
    public void LogTrace(string message, params object[] args) => this.Capture(message, args);
    public void LogWarning(string message) => this.Capture(message);
    public void LogWarning(string message, params object[] args) => this.Capture(message, args);

    private void Capture(string message, object[] args = null, string exception = null)
    {
        String renderedArguments = args is { Length: > 0 } ? String.Join(" ", args) : String.Empty;
        this.Messages.Add(String.Join(" ", new[] { message, renderedArguments, exception }.Where(value => !String.IsNullOrEmpty(value))));
    }
}
