using System;
using System.Collections.Generic;
using System.Linq;
using UrbanZenith.Interfaces;
using Spectre.Console;

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
            AnsiConsole.MarkupLine("\n[grey]Press Enter to return to the main menu...[/]");
            Console.ReadLine();
        }

        private void ShowHelp()
        {
            AnsiConsole.Clear();

            AnsiConsole.Write(
                new Rule("[bold yellow]Available Commands[/]")
                    .RuleStyle("grey")
                    .Centered());

            var table = new Table()
                .Border(TableBorder.Square)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("[bold Blue]Command[/]"))
                .AddColumn(new TableColumn("[bold Blue]Description[/]"));

            foreach (var cmd in _availableCommands.OrderBy(c => c.Name))
            {
                table.AddRow($"[cyan]{cmd.Name}[/]", $"[white]{cmd.Description}[/]");
            }

            table.AddRow("[red]exit[/]", "[grey]Quits the application.[/]");
            table.AddRow("[red]quit[/]", "[grey]Quits the application.[/]");

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
    }
}