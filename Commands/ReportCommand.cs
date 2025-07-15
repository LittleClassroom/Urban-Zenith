using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;

namespace UrbanZenith.Commands
{
    public class ReportCommand : ICommand, IMenuCommand
    {
        public string Name => "report";
        public string Description => "Generate business reports. Example: 'report daily', 'report method'";

        public void Execute(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                ShowMenu();
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
                        ReportService.ShowDailySalesReport(); // today
                    }
                    else
                    {
                        Console.WriteLine("Usage: report daily [yyyy-mm-dd]");
                    }
                    break;

                case "items":
                case "top-items":
                    ReportService.ShowTopSellingItems();
                    break;

                case "method":
                    ReportService.ShowSalesByPaymentMethod();
                    break;

                default:
                    Console.WriteLine($"Unknown report type: {subCommand}");
                    ShowHelp();
                    break;
            }
        }

        public void ShowMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Report Menu ===");
                Console.WriteLine("1. Daily Sales Report");
                Console.WriteLine("2. Daily Sales for Specific Date");
                Console.WriteLine("3. Sales by Payment Method");
                Console.WriteLine("4. Top Selling Menu Items");
                Console.WriteLine("0. Back to Main Menu");
                Console.Write("Select an option: ");

                string input = Console.ReadLine()?.Trim();
                if (input == "0") break;

                try
                {
                    switch (input)
                    {
                        case "1":
                            ReportService.ShowDailySalesReport(); // today
                            break;

                        case "2":
                            Console.Write("Enter date (YYYY-MM-DD): ");
                            string dateInput = Console.ReadLine()?.Trim();
                            if (DateTime.TryParse(dateInput, out DateTime date))
                                ReportService.ShowDailySalesReport(date);
                            else
                                Console.WriteLine("Invalid date format.");
                            break;

                        case "3":
                            ReportService.ShowSalesByPaymentMethod();
                            break;

                        case "4":
                            ReportService.ShowTopSellingItems();
                            break;

                        default:
                            Console.WriteLine("Invalid option.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] {ex.Message}");
                }
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  report daily               - Show today’s sales summary");
            Console.WriteLine("  report daily <YYYY-MM-DD> - Show sales summary for a specific day");
            Console.WriteLine("  report method              - Show revenue grouped by payment method");
            Console.WriteLine("  report items               - Show quantity sold per menu item");
        }
    }
}
