namespace OnlineShopping.Utilities;

public static class InputHelper
{
    public static void Pause(string message = "Press Enter to continue...")
    {
        Console.WriteLine();
        Console.Write(message);
        Console.ReadLine();
    }

    public static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            Console.WriteLine("Input cannot be empty.");
        }
    }

    public static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (int.TryParse(input, out var value) && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine($"Enter a valid integer between {min} and {max}.");
        }
    }

    public static decimal ReadDecimal(string prompt, decimal min = decimal.MinValue, decimal max = decimal.MaxValue)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (decimal.TryParse(input, out var value) && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine($"Enter a valid number between {min} and {max}.");
        }
    }

    public static decimal? ReadOptionalDecimal(string prompt)
    {
        return ReadOptionalDecimal(prompt, decimal.MinValue, decimal.MaxValue);
    }

    public static decimal? ReadOptionalDecimal(string prompt, decimal min, decimal max)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (decimal.TryParse(input, out var value) && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine($"Enter a valid number between {min} and {max}, or leave blank.");
        }
    }
}
