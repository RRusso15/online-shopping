using OnlineShopping.Interfaces;
using OnlineShopping.Models;
using OnlineShopping.Utilities;

namespace OnlineShopping.Menus;

public sealed class MainMenu
{
    private readonly IAuthService _authService;
    private readonly CustomerMenu _customerMenu;
    private readonly AdminMenu _adminMenu;

    public MainMenu(IAuthService authService, CustomerMenu customerMenu, AdminMenu adminMenu)
    {
        _authService = authService;
        _customerMenu = customerMenu;
        _adminMenu = adminMenu;
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

            var choice = InputHelper.ReadInt("Choose an option: ", 1, 3);

            switch (choice)
            {
                case 1:
                    Register();
                    InputHelper.Pause();
                    break;
                case 2:
                    Login();
                    InputHelper.Pause();
                    break;
                case 3:
                    Console.WriteLine("Goodbye.");
                    return;
            }
        }
    }

    private void Register()
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
        Console.WriteLine(message);
        if (success)
        {
            Console.WriteLine("You can now login.");
        }
    }

    private void Login()
    {
        Console.Clear();
        Console.WriteLine("=== LOGIN ===");
        var username = InputHelper.ReadRequiredString("Username: ");
        var password = InputHelper.ReadRequiredString("Password: ");

        var user = _authService.Login(username, password);
        if (user is null)
        {
            Console.WriteLine("Invalid credentials.");
            return;
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
    }
}
