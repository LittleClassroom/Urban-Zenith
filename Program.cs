using System;
using System.Collections.Generic;
using System.Linq;
using UrbanZenith.Interfaces;
using UrbanZenith.Commands;
using UrbanZenith.Database;

namespace UrbanZenith
{
    public static class Program
    {
        private static readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

        private static void RegisterCommands()
        {
            var commandList = new List<ICommand>
            {
                new MenuCommand(),
                new OrderCommand(),
                new OrderItemCommand(),
                new TableCommand(),
                new StaffCommand(),
                new PaymentCommand(),
                new ReportCommand(),
            };

            commandList.Add(new HelpCommand(commandList));

            foreach (var cmd in commandList)
            {
                _commands[cmd.Name.ToLower()] = cmd;
            }
        }

        public static void Main(string[] args)
        {

            Console.WriteLine("Initializing database...");
            DatabaseContext.Initialize();
            RegisterCommands();
            Console.WriteLine("=== Welcome to Urban Zenith Restaurant CLI ===");
            Console.WriteLine("Type 'help' to see available commands.");
            Console.WriteLine("Type 'exit' to quit.\n");

            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine().Trim();

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
                    Console.WriteLine($"Unknown command: {cmdName}");
                }
            }

            Console.WriteLine("Exiting... Goodbye!");
        }
    }
}
