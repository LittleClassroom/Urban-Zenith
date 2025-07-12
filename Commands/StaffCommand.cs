using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;

namespace UrbanZenith.Commands
{
    public class StaffCommand : ICommand
    {
        public string Name => "staff";
        public string Description => "Manage staff members (list, add, remove).";

        public void Execute(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                ShowHelp();
                return;
            }

            var parts = args.Split(' ', 2);
            string subcommand = parts[0].ToLower();
            string subArgs = parts.Length > 1 ? parts[1] : "";

            switch (subcommand)
            {
                case "list":
                    StaffService.ListStaff();
                    break;

                case "add":
                    StaffService.AddStaff();
                    break;

                case "remove":
                    if (int.TryParse(subArgs, out int removeId))
                        StaffService.RemoveStaff(removeId);
                    else
                        Console.WriteLine("Usage: staff remove <id>");
                    break;

                case "info":
                    if (int.TryParse(subArgs, out int infoId))
                        StaffService.GetStaffInfo(infoId);
                    else
                        Console.WriteLine("Usage: staff info <id>");
                    break;

                case "update":
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int updateId))
                        StaffService.UpdateStaff(updateId);
                    else
                        Console.WriteLine("Usage: staff update <id>");
                    break;


                default:
                    Console.WriteLine($"Unknown staff command: '{subcommand}'");
                    ShowHelp();
                    break;
            }
        }

        private void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  staff list");
            Console.WriteLine("  staff add");
            Console.WriteLine("  staff remove <id>");
            Console.WriteLine("  staff info <id>");
            Console.WriteLine("  staff update <id>");

        }
    }
}
