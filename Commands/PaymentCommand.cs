using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;

namespace UrbanZenith.Commands
{
    public class PaymentCommand : ICommand
    {
        public string Name => "payment";
        public string Description => "Process payment or view payment history/details.";

        public void Execute(string args)
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

                default:
                    Console.WriteLine($"Unknown command: {command}");
                    ShowHelp();
                    break;
            }
        }

        private void ProcessPayment(string[] parts)
        {
            if (parts.Length != 2 || !int.TryParse(parts[1], out int tableId))
            {
                Console.WriteLine("Usage: payment process <tableId>");
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
                // default: page 1, 10 records
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
                Console.WriteLine("Usage: payment history [<page>] [<pageSize>]");
                return;
            }

            PaymentService.ShowPaymentHistory(page, pageSize);
        }

        private void ShowPaymentInfo(string[] parts)
        {
            if (parts.Length != 2 || !int.TryParse(parts[1], out int paymentId))
            {
                Console.WriteLine("Usage: payment info <paymentId>");
                return;
            }

            PaymentService.ShowPaymentDetail(paymentId);
        }

        private void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  payment process <tableId>          - Process a payment for a table");
            Console.WriteLine("  payment history                    - Show latest 10 payment records (page 1)");
            Console.WriteLine("  payment history <page>             - Show payment records for specified page");
            Console.WriteLine("  payment history <page> <pageSize>  - Show records with custom page size");
            Console.WriteLine("  payment info <paymentId>           - Show detailed info about a payment");
        }
    }
}
