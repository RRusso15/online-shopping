using OnlineShopping.Interfaces;
using OnlineShopping.Models;
using OnlineShopping.Utilities;

namespace OnlineShopping.Menus;

public sealed class MainMenu
{
    private readonly IAuthService _authService;
    private readonly CustomerMenu _customerMenu;
    private readonly AdminMenu _adminMenu;
    private readonly Dictionary<int, ICommand> _commands;

    public MainMenu(IAuthService authService, CustomerMenu customerMenu, AdminMenu adminMenu)
    {
        _authService = authService;
        _customerMenu = customerMenu;
        _adminMenu = adminMenu;

        _commands = new Dictionary<int, ICommand>
        {
            [1] = new DelegateCommand("Register", Register),
            [2] = new DelegateCommand("Login", Login),
            [3] = new DelegateCommand("Exit", () => CommandResult.Exit("Goodbye."))
        };
    }

    public void Run()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== MAIN MENU ===");
            Console.WriteLine("1. Register");
            Console.WriteLine("2. Login");
            Console.WriteLine("3. Exit");

            var choice = InputHelper.ReadInt("Choose an option: ", 1, _commands.Count);
            var result = ExecuteCommand(_commands[choice]);
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                Console.WriteLine(result.Message);
            }

            if (result.ShouldExit)
            {
                return;
            }

            InputHelper.Pause();
        }
    }

    private static CommandResult ExecuteCommand(ICommand command)
    {
        try
        {
            return command.Execute();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is KeyNotFoundException || ex is FormatException)
        {
            return CommandResult.Fail($"Error: {ex.Message}");
        }
    }

    private CommandResult Register()
    {
        Console.WriteLine();
        Console.WriteLine("=== REGISTER ===");
        var username = InputHelper.ReadRequiredString("Username: ");
        var password = InputHelper.ReadRequiredString("Password: ");

        Console.WriteLine("Role:");
        Console.WriteLine("1. Customer");
        Console.WriteLine("2. Administrator");
        var roleChoice = InputHelper.ReadInt("Choose role: ", 1, 2);
        var role = roleChoice == 2 ? UserRole.Administrator : UserRole.Customer;

        var success = _authService.Register(username, password, role, out var message);
        if (success)
        {
            return CommandResult.Ok($"{message}{Environment.NewLine}You can now login.");
        }

        return CommandResult.Fail(message);
    }

    private CommandResult Login()
    {
        Console.Clear();
        Console.WriteLine("=== LOGIN ===");
        var username = InputHelper.ReadRequiredString("Username: ");
        var password = InputHelper.ReadRequiredString("Password: ");

        var user = _authService.Login(username, password);
        if (user is null)
        {
            return CommandResult.Fail("Invalid credentials.");
        }

        Console.WriteLine($"Welcome {user.Username} ({user.Role}).");

        if (user is Customer customer)
        {
            _customerMenu.Run(customer);
        }
        else if (user is Administrator admin)
        {
            _adminMenu.Run(admin);
        }

        return CommandResult.Ok();
    }
}
