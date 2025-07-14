using System;
using System.Collections.Generic;
using System.Linq;
using UrbanZenith.Interfaces;

namespace UrbanZenith.Commands
{
    public class HelpCommand : ICommand, IMenuCommand
    {
        public string Name => "help";
        public string Description => "Displays a list of available commands.";
        private readonly IEnumerable<ICommand> _availableCommands;

        public HelpCommand(IEnumerable<ICommand> availableCommands)
        {
            _availableCommands = availableCommands;
        }

        public void Execute(string args)
        {
            ShowHelp();
        }

        public void ShowMenu()
        {
            ShowHelp();
            Console.WriteLine("Press Enter to return to the main menu...");
            Console.ReadLine();
        }

        private void ShowHelp()
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("-------------------");
            foreach (var cmd in _availableCommands.OrderBy(c => c.Name))
            {
                Console.WriteLine($"  {cmd.Name,-10} - {cmd.Description}");
            }
            Console.WriteLine($"  {"exit",-10} - Quits the application.\n");
        }
    }
}
