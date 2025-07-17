using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;
using Spectre.Console;

namespace UrbanZenith.Commands
{
    public class PaymentCommand : ICommand, IMenuCommand
    {
        public string Name => "payment";
        public string Description => "Process payment or view payment history/details.";

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
                var command = parts[0].ToLower();

                switch (command)
                {
                    case "process":
                        ProcessPayment(parts);
                        break;

                    case "history":
                        ShowPaymentHistory(parts);
                        break;

                    case "info":
                        ShowPaymentInfo(parts);
                        break;

                    case "help":
                        ShowHelp();
                        break;

                    default:
                        AnsiConsole.MarkupLine($"[red][[!]] Unknown command: '[bold red]{command}[/]'[/]");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] [red]{ex.Message}[/]");
            }
        }

        private void ProcessPayment(string[] parts)
        {
            if (parts.Length != 2 || !int.TryParse(parts[1], out int tableId))
            {
                AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]payment process <tableId>[/]");
                return;
            }

            PaymentService.ProcessPaymentWithPrompt(tableId);
        }

        private void ShowPaymentHistory(string[] parts)
        {
            int page = 1;
            int pageSize = 10;

            if (parts.Length == 1)
            {

            }
            else if (parts.Length == 2 && int.TryParse(parts[1], out int parsedPage))
            {
                page = parsedPage;
            }
            else if (parts.Length == 3 &&
                     int.TryParse(parts[1], out parsedPage) &&
                     int.TryParse(parts[2], out int parsedSize))
            {
                page = parsedPage;
                pageSize = parsedSize;
            }
            else
            {
                AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]payment history [<page>] [<pageSize>][/]");
                return;
            }

            PaymentService.ShowPaymentHistory(page, pageSize);
        }

        private void ShowPaymentInfo(string[] parts)
        {
            if (parts.Length != 2 || !int.TryParse(parts[1], out int paymentId))
            {
                AnsiConsole.MarkupLine("[red][[!]][/] Usage: [bold white]payment info <paymentId>[/]");
                return;
            }

            PaymentService.ShowPaymentDetail(paymentId);
        }

        public void ShowMenu()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(
                    new Rule("[bold yellow]Payment Management Menu[/]")
                        .RuleStyle("grey")
                        .Centered());

                AnsiConsole.MarkupLine("[bold green]1.[/] Process payment");
                AnsiConsole.MarkupLine("[bold green]2.[/] View payment history");
                AnsiConsole.MarkupLine("[bold green]3.[/] View payment details");
                AnsiConsole.MarkupLine("[bold red]0.[/] Back to main menu");

                string input = AnsiConsole.Prompt(
                    new TextPrompt<string>("[bold blue]Choose an option:[/]")
                        .ValidationErrorMessage("[red][[!]] Invalid choice. Please enter a number from the list.[/]")
                        .Validate(val =>
                        {
                            if (string.IsNullOrEmpty(val)) return ValidationResult.Error("[red]Choice cannot be empty.[/]");
                            return int.TryParse(val, out int num) && (num >= 0 && num <= 3)
                                ? ValidationResult.Success()
                                : ValidationResult.Error("[red][[!]] Invalid choice. Please enter 0, 1, 2, or 3.[/]");
                        }));

                if (input == "0") break;

                try
                {
                    switch (input)
                    {
                        case "1":
                            int tableId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter Table ID:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid Table ID. Please enter a positive number.[/]")
                                    .Validate(val => val > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Table ID must be a positive number.[/]")));
                            PaymentService.ProcessPaymentWithPrompt(tableId);
                            break;

                        case "2":
                            int page = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter page number (default 1):[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid page number. Please enter a positive number.[/]")
                                    .Validate(val => val >= 1 ? ValidationResult.Success() : ValidationResult.Error("[red]Page number must be 1 or greater.[/]"))
                                    .DefaultValue(1));

                            int pageSize = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter page size (default 10):[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid page size. Please enter a positive number.[/]")
                                    .Validate(val => val > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Page size must be a positive number.[/]"))
                                    .DefaultValue(10));

                            PaymentService.ShowPaymentHistory(page, pageSize);
                            break;

                        case "3":
                            int paymentId = AnsiConsole.Prompt(
                                new TextPrompt<int>("[green]Enter Payment ID:[/]")
                                    .ValidationErrorMessage("[red][[!]] Invalid Payment ID. Please enter a positive number.[/]")
                                    .Validate(val => val > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Payment ID must be a positive number.[/]")));
                            PaymentService.ShowPaymentDetail(paymentId);
                            break;

                        default:
                            AnsiConsole.MarkupLine("[red][[!]] Invalid option. Please enter a number from the menu.[/]");
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
                new Rule("[bold yellow]Payment Command Usage[/]")
                    .RuleStyle("grey")
                    .Centered());

            var table = new Table()
                .Border(TableBorder.Square)
                .BorderColor(Color.Blue)
                .AddColumn(new TableColumn("[bold blue]Command[/]"))
                .AddColumn(new TableColumn("[bold blue]Description[/]"));

            table.AddRow("[cyan]payment process[/] [grey]<tableId>[/]", "[white]Process a payment for a table.[/]");
            table.AddRow("[cyan]payment history[/]", "[white]Show latest 10 payment records (page 1).[/]");
            table.AddRow("[cyan]payment history[/] [grey]<page>[/]", "[white]Show payment records for specified page (default size 10).[/]");
            table.AddRow("[cyan]payment history[/] [grey]<page> <pageSize>[/]", "[white]Show records with custom page number and size.[/]");
            table.AddRow("[cyan]payment info[/] [grey]<paymentId>[/]", "[white]Show detailed information about a payment.[/]");
            table.AddRow("[cyan]payment help[/]", "[white]Display this help message.[/]");

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }
    }
}