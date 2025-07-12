using System;
using UrbanZenith.Interfaces;

namespace UrbanZenith.Commands
{
    public class ReportCommand : ICommand
    {
        public string Name => "report";
        public string Description => "Generate reports (e.g., 'report daily', 'report revenue').";

        public void Execute(string args)
        {
            Console.WriteLine($"Executing Report Command with args: '{args}'");
            // Future logic: Parse 'args' to generate different types of reports.
        }
    }
}