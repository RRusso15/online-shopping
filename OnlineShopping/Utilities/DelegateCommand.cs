using OnlineShopping.Interfaces;

namespace OnlineShopping.Utilities;

/// <summary>
/// Delegate-backed command implementation for role menu composition.
/// </summary>
public sealed class DelegateCommand : ICommand
{
    private readonly Func<CommandResult> _execute;

    public DelegateCommand(string label, Func<CommandResult> execute)
    {
        Label = label;
        _execute = execute;
    }

    public string Label { get; }

    public CommandResult Execute() => _execute();
}
