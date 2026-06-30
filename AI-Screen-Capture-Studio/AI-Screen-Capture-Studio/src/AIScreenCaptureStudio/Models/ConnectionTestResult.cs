namespace AIScreenCaptureStudio.Models;

public sealed class ConnectionTestResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ConnectionTestResult Ok(string message = "Connected") => new() { Success = true, Message = message };
    public static ConnectionTestResult Fail(string message) => new() { Success = false, Message = message };
}
