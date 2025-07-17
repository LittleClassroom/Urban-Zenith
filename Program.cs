using System;
using System.Collections.Generic;
using UrbanZenith.Interfaces;
using UrbanZenith.Commands;
using UrbanZenith.Database;
using Spectre.Console;

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

            _commandList.Add(new HelpCommand(_commandList));

            foreach (var cmd in _commandList)
            {
                _commands[cmd.Name.ToLower()] = cmd;
            }
        }

        public static void Main(string[] args)
        {
            AnsiConsole.Write(
                new FigletText("Urban Zenith")
                    .LeftJustified()
                    .Color(Color.Yellow));

            AnsiConsole.Write(
                new Rule("[yellow]Restaurant CLI[/]")
                    .RuleStyle("grey")
                    .Centered());

            DatabaseContext.Initialize();
            System.Threading.Thread.Sleep(500);

            RegisterCommands();

            int mode = PromptMode();

            if (mode == 1)
                RunTextCommandMode();
            else if (mode == 2)
                RunMenuMode();

            AnsiConsole.Write(
                new Align(
                    new Panel("[cyan]Thank you for using Urban Zenith. Goodbye![/]")
                        .Border(BoxBorder.Double)
                        .BorderColor(Color.Grey),
                    HorizontalAlignment.Center));
        }

        private static int PromptMode()
        {
            var promptContentString = "[bold green]Choose interface mode:[/]\n" +
                                      "  [yellow]1[/] - Text Command Mode\n" +
                                      "  [yellow]2[/] - Menu Navigation Mode";

            var panel = new Panel(new Markup(promptContentString))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Header("[yellow]Mode Selection[/]")
                .HeaderAlignment(Justify.Center)
                .Padding(1, 1, 1, 1);

            AnsiConsole.Write(panel);

            return AnsiConsole.Prompt(
                new TextPrompt<int>("[blue][[ Select Mode ]][/]: ")
                    .ValidationErrorMessage("[red][[!]] Invalid input. Please enter 1 or 2.[/]")
                    .Validate(input => input == 1 || input == 2));
        }

        private static string ReadInput(string label)
        {
            return AnsiConsole.Ask<string>($"[yellow]{label}[/] [green]>[/]");
        }

        private static void RunTextCommandMode()
        {
            AnsiConsole.Clear();

            AnsiConsole.Write(
                new Rule("[bold yellow] Text Command Mode [/]")
                    .RuleStyle("grey")
                    .Centered());

            var instructionsPanel = new Panel(
                new Markup(
                    "[cyan]Type '[bold white]help[/]' to see available commands.\n" +
                    "Type '[bold white]exit[/]' or '[bold white]quit[/]' to return to mode selection.[/]"
                ))
                .Border(BoxBorder.Square)
                .BorderColor(Color.Grey)
                .Padding(1, 1, 1, 1);

            AnsiConsole.Write(instructionsPanel);

            AnsiConsole.Write(new Rule().RuleStyle("grey"));

            AnsiConsole.WriteLine();

            while (true)
            {
                var input = ReadInput("Command").Trim();

                if (string.IsNullOrEmpty(input))
                    continue;

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    break;

                var parts = input.Split(' ', 2);
                var cmdName = parts[0].ToLower();
                var cmdArgs = parts.Length > 1 ? parts[1] : "";

                if (_commands.TryGetValue(cmdName, out var cmd))
                {
                    try
                    {
                        cmd.Execute(cmdArgs);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]{ex.Message}[/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red][[!]] Unknown command:[/] '[bold red]{cmdName}[/]'. Type 'help' for a list of available commands.");
                }
            }

            AnsiConsole.MarkupLine("[cyan]\nReturning to mode selection...[/]");
        }

        private static void RunMenuMode()
        {
            AnsiConsole.Clear();

            AnsiConsole.Write(
                new Rule("[bold yellow]Menu Navigation Mode[/]") // Improved header styling
                    .RuleStyle("grey")
                    .Centered());

            AnsiConsole.MarkupLine("[cyan]Select a command to execute:[/]\n"); // Moved description out of table

            while (true)
            {
                var table = new Table().Border(TableBorder.Heavy).BorderColor(Color.Green);
                table.AddColumn(new TableColumn("[purple]No.[/]").Centered());
                table.AddColumn("[white]Command[/]");
                table.AddColumn("[grey]Description[/]");

                for (int i = 0; i < _commandList.Count; i++)
                {
                    table.AddRow(
                        $"[magenta]{i + 1}[/]",
                        $"[white]{_commandList[i].Name}[/]",
                        $"[grey]{_commandList[i].Description}[/]");
                }

                table.AddRow("[red]0[/]", "[red]Exit Menu[/]", "[red]Return to main mode selection[/]");
                AnsiConsole.Write(table);

                var input = AnsiConsole.Prompt(
                    new TextPrompt<int>("[green][[ Select Command ]][/]:") // Improved prompt text
                        .ValidationErrorMessage("[red][[!]] Invalid choice. Please enter a valid number.[/]")); // Consistent error message

                if (input == 0)
                    break;

                if (input > 0 && input <= _commandList.Count)
                {
                    var cmd = _commandList[input - 1];
                    if (cmd is IMenuCommand menu)
                    {
                        try
                        {
                            AnsiConsole.WriteLine();
                            menu.ShowMenu();
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[bold red]ERROR:[/][red] {ex.Message}[/]");
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[yellow]'{cmd.Name}' does not support menu mode directly. Please use Text Command Mode for this.[/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[red][[!]] Invalid choice. Please select a number from the list.[/]"); // Consistent error message
                }

                AnsiConsole.WriteLine();
            }

            AnsiConsole.MarkupLine("[cyan]\nReturning to mode selection...[/]"); // Consistent exit message
        }
    }
}