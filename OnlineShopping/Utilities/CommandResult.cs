namespace OnlineShopping.Utilities;

/// <summary>
/// Standardized command execution outcome used by menu dispatchers.
/// </summary>
public sealed class CommandResult
{
    private CommandResult(bool shouldExit, bool success, string? message)
    {
        ShouldExit = shouldExit;
        Success = success;
        Message = message;
    }

    public bool ShouldExit { get; }
    public bool Success { get; }
    public string? Message { get; }

    public static CommandResult Ok(string? message = null) => new(false, true, message);
    public static CommandResult Exit(string? message = null) => new(true, true, message);
    public static CommandResult Fail(string message) => new(false, false, message);
}
