using System;
using UrbanZenith.Interfaces;

namespace UrbanZenith.Commands
{
    public class PaymentCommand : ICommand
    {
        public string Name => "payment";
        public string Description => "Process a payment for a table (e.g., 'payment process 5').";

        public void Execute(string args)
        {
            Console.WriteLine($"Executing Payment Command with args: '{args}'");
            // Future logic: Parse 'args' to get table number and process payment.
        }
    }
}