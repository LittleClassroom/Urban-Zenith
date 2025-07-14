using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;

namespace UrbanZenith.Commands
{
    public class ReportCommand : ICommand
    {
        public string Name => "report";
        public string Description => "Generate business reports. Example: 'report daily', 'report method'";

        public void Execute(string args)
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
                        ReportService.ShowDailySalesReport(); // default = today
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

        private void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  report daily               - Show today’s sales summary");
            Console.WriteLine("  report daily <YYYY-MM-DD> - Show sales summary for a specific day");
            Console.WriteLine("  report method              - Show revenue grouped by payment method");
            Console.WriteLine("  report items                - Show quantity sold per menu item");
        }
    }
}
