using OnlineShopping.Utilities;

namespace OnlineShopping.Interfaces;

/// <summary>
/// Represents a menu command that encapsulates one user action.
/// </summary>
public interface ICommand
{
    string Label { get; }
    CommandResult Execute();
}
