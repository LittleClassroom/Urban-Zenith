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
            AnsiConsole.MarkupLine("\n[yellow]Press Enter to return to the main menu...[/]");
            Console.ReadLine();
        }

        private void ShowHelp()
        {
            AnsiConsole.Clear();

            AnsiConsole.Write(
                new FigletText("Help")
                    .LeftJustified()
                    .Color(Color.Blue));

            var table = new Table()
                .Border(TableBorder.Rounded)
                .Title("[green]Available Commands[/]")
                .AddColumn(new TableColumn("[bold teal]Command[/]").Centered())
                .AddColumn(new TableColumn("[bold yellow]Description[/]").LeftAligned());

            foreach (var cmd in _availableCommands.OrderBy(c => c.Name))
            {
                table.AddRow($"[purple]{cmd.Name}[/]", $"[silver]{cmd.Description}[/]");
            }

            table.AddRow("[red]exit[/]", "[silver]Quits the application.[/]");

            AnsiConsole.Write(table);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Type a command name and press Enter to execute it.[/]");
        }
    }
}