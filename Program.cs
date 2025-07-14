using System;
using System.Collections.Generic;
using System.Linq;
using UrbanZenith.Interfaces;
using UrbanZenith.Commands;
using UrbanZenith.Database;
using System.ComponentModel.Design;

namespace UrbanZenith
{
    public static class Program
    {
        private static readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();
        private static List<ICommand> _commandList = new List<ICommand>();

        private static void RegisterCommands()
        {
            _commandList = new List<ICommand>
            {
                new ItemMenuCommand(),
                new OrderCommand(),
                new TableCommand(),
                new StaffCommand(),
                new PaymentCommand(),
                new ReportCommand()
            };

            // Add help last so it sees all other commands
            _commandList.Add(new HelpCommand(_commandList));

            foreach (var cmd in _commandList)
            {
                _commands[cmd.Name.ToLower()] = cmd;
            }
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Initializing database...");
            DatabaseContext.Initialize();
            RegisterCommands();

            Console.WriteLine("=== Welcome to Urban Zenith Restaurant CLI ===\n");

            int mode = PromptMode();

            if (mode == 1)
                RunTextCommandMode();
            else if (mode == 2)
                RunMenuMode();
        }

        private static int PromptMode()
        {
            Console.WriteLine("Choose interface mode:");
            Console.WriteLine("[1] Text Command Mode");
            Console.WriteLine("[2] Menu Navigation Mode");

            while (true)
            {
                Console.Write("Enter your choice (1 or 2): ");
                string input = Console.ReadLine()?.Trim();

                if (input == "1" || input.Equals("text", StringComparison.OrdinalIgnoreCase))
                    return 1;
                if (input == "2" || input.Equals("menu", StringComparison.OrdinalIgnoreCase))
                    return 2;

                Console.WriteLine("Invalid input. Please enter 1 or 2.");
            }
        }

        private static void RunTextCommandMode()
        {
            Console.WriteLine("\n--- Text Command Mode ---");
            Console.WriteLine("Type 'help' to see available commands.");
            Console.WriteLine("Type 'exit' to quit.\n");

            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input))
                    continue;

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    break;

                string[] parts = input.Split(' ', 2);
                string cmdName = parts[0].ToLower();
                string cmdArgs = parts.Length > 1 ? parts[1] : "";

                if (_commands.TryGetValue(cmdName, out var cmd))
                {
                    try
                    {
                        cmd.Execute(cmdArgs);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Unknown command: '{cmdName}'. Type 'help' for a list of available commands.");
                }
            }

            Console.WriteLine("Exiting... Goodbye!");
        }

        private static void RunMenuMode()
        {
            Console.WriteLine("\n--- Menu Navigation Mode ---");
            Console.WriteLine("Select a command to execute:\n");

            while (true)
            {
                for (int i = 0; i < _commandList.Count; i++)
                {
                    Console.WriteLine($"[{i + 1}] {_commandList[i].Name} - {_commandList[i].Description}");
                }

                Console.WriteLine("[0] Exit");
                Console.Write("\nChoose a command number: ");
                string input = Console.ReadLine()?.Trim();

                if (input == "0")
                    break;

                if (int.TryParse(input, out int index) && index > 0 && index <= _commandList.Count)
                {
                    var cmd = _commandList[index - 1];
                    if (cmd is IMenuCommand menu)
                    {
                        try
                        {
                            menu.ShowMenu();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[ERROR] {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"'{cmd.Name}' does not support menu mode.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid choice.");
                }

                Console.WriteLine(); 
            }

            Console.WriteLine("Exiting... Goodbye!");
        }

    }
}
