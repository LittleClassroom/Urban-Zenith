using System;
using UrbanZenith.Interfaces;
using UrbanZenith.Services;

namespace UrbanZenith.Commands
{
    public class StaffCommand : ICommand, IMenuCommand
    {
        public string Name => "staff";
        public string Description => "Manage staff members (list, add, remove, info, update)";

        public void Execute(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                ShowMenu();
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
                    if (int.TryParse(subArgs, out int updateId))
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

        public void ShowMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== Staff Management Menu ===");
                Console.WriteLine("1. List all staff");
                Console.WriteLine("2. Add new staff member");
                Console.WriteLine("3. Remove staff member");
                Console.WriteLine("4. View staff info");
                Console.WriteLine("5. Update staff details");
                Console.WriteLine("0. Back to main menu");
                Console.Write("Choose an option: ");

                string input = Console.ReadLine()?.Trim();
                if (input == "0") break;

                try
                {
                    switch (input)
                    {
                        case "1":
                            StaffService.ListStaff();
                            break;
                        case "2":
                            StaffService.AddStaff();
                            break;
                        case "3":
                            Console.Write("Enter Staff ID to remove: S-");
                            if (int.TryParse(Console.ReadLine(), out int removeId))
                                StaffService.RemoveStaff(removeId);
                            else
                                Console.WriteLine($"Invalid ID. S-{removeId}");
                            break;
                        case "4":
                            Console.Write("Enter Staff ID to view: S-");
                            if (int.TryParse(Console.ReadLine(), out int infoId))
                                StaffService.GetStaffInfo(infoId);
                            else
                                Console.WriteLine($"Invalid ID. S-{infoId}");
                            break;
                        case "5":
                            Console.Write("Enter Staff ID to update: ");
                            if (int.TryParse(Console.ReadLine(), out int updateId))
                                StaffService.UpdateStaff(updateId);
                            else
                                Console.WriteLine("Invalid ID.");
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
            Console.WriteLine("  staff list");
            Console.WriteLine("  staff add");
            Console.WriteLine("  staff remove <id>");
            Console.WriteLine("  staff info <id>");
            Console.WriteLine("  staff update <id>");
        }
    }
}
