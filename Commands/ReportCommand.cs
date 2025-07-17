using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;
using Spectre.Console;

namespace UrbanZenith.Commands
{
    public class ReportCommand : ICommand, IMenuCommand
    {
        public string Name => "report";
        public string Description => "Generate business reports. Example: 'report daily', 'report method'";

        public void Execute(string args)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(args))
                {
                    ShowHelp();
                    return;
                }

                var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var subCommand = parts[0].ToLower();

                switch (subCommand)
                {
                    case "daily":
                        if (parts.Length == 2 && DateTime.TryParse(parts[1], out DateTime date))
                        {
                            ReportService.ShowDailySalesReport(date);
                        }
                        else if (parts.Length == 1)
                        {
                            ReportService.ShowDailySalesReport();
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]report daily [yyyy-mm-dd][/]");
                        }
                        break;

                    case "items":
                    case "top-items":
                        ReportService.ShowTopSellingItems();
                        break;

                    case "method":
                        ReportService.ShowSalesByPaymentMethod();
                        break;

                    case "help":
                        ShowHelp();
                        break;

                    default:
                        AnsiConsole.MarkupLine($"[red][[!]] Unknown report type: '[bold red]{subCommand}[/]'[/]");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]{ex.Message}[/]");
            }
        }

        public void ShowMenu()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(
                    new Rule("[bold yellow]Report Generation Menu[/]")
                        .RuleStyle("grey")
                        .Centered());

                AnsiConsole.MarkupLine("[bold green]1.[/] Daily Sales Report (Today)");
                AnsiConsole.MarkupLine("[bold green]2.[/] Daily Sales Report (Specific Date)");
                AnsiConsole.MarkupLine("[bold green]3.[/] Sales by Payment Method");
                AnsiConsole.MarkupLine("[bold green]4.[/] Top Selling Menu Items");
                AnsiConsole.MarkupLine("[bold red]0.[/] Back to Main Menu");

                string input = AnsiConsole.Prompt(
                    new TextPrompt<string>("[bold blue]Select an option:[/]")
                        .ValidationErrorMessage("[red][[!]] Invalid option. Please enter a number from the list.[/]")
                        .Validate(val =>
                        {
                            if (string.IsNullOrEmpty(val)) return ValidationResult.Error("[red]Option cannot be empty.[/]");
                            return int.TryParse(val, out int num) && (num >= 0 && num <= 4)
                                ? ValidationResult.Success()
                                : ValidationResult.Error("[red][[!]] Invalid option. Please enter 0, 1, 2, 3, or 4.[/]");
                        }));

                if (input == "0") break;

                try
                {
                    switch (input)
                    {
                        case "1":
                            ReportService.ShowDailySalesReport();
                            break;

                        case "2":
                            DateTime date = AnsiConsole.Prompt(
                                new TextPrompt<DateTime>("[green]Enter date (YYYY-MM-DD):[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid date format. Please use YYYY-MM-DD.[/]")
                                    .PromptStyle("green")
                                    .Validate<DateTime>(d =>
                                    {
                                        if (d == default(DateTime)) return ValidationResult.Error("[red]Date cannot be empty.[/]");
                                        return ValidationResult.Success();
                                    }));
                            ReportService.ShowDailySalesReport(date);
                            break;

                        case "3":
                            ReportService.ShowSalesByPaymentMethod();
                            break;

                        case "4":
                            ReportService.ShowTopSellingItems();
                            break;

                        default:
                            AnsiConsole.MarkupLine("[red][[!]] Invalid option. Please select an option from the menu.[/]");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]{ex.Message}[/]");
                }

                AnsiConsole.MarkupLine("\n[grey]Press Enter to continue...[/]");
                Console.ReadLine();
            }
        }

        private void ShowHelp()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new Rule("[bold yellow]Report Command Usage[/]")
                    .RuleStyle("grey")
                    .Centered());

            var table = new Table()
                .Border(TableBorder.Square)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("[bold blue]Command[/]"))
                .AddColumn(new TableColumn("[bold blue]Description[/]"));

            table.AddRow("[cyan]report daily[/]", "[white]Show today's sales summary.[/]");
            table.AddRow("[cyan]report daily[/] [grey]<YYYY-MM-DD>[/]", "[white]Show sales summary for a specific day.[/]");
            table.AddRow("[cyan]report method[/]", "[white]Show revenue grouped by payment method.[/]");
            table.AddRow("[cyan]report items[/]", "[white]Show quantity sold per menu item.[/]");
            table.AddRow("[cyan]report help[/]", "[white]Display this help message.[/]");

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
    }
}