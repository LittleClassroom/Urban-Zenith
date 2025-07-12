using System;
using System.Data.SQLite;
using UrbanZenith.Database;

namespace UrbanZenith.Services
{
    public static class TableService
    {
        public static void ListTables()
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT t.Id, t.Name, t.Type, t.Status, s.Name as StaffName
                FROM Tables t
                LEFT JOIN Staff s ON t.StaffId = s.Id
                ORDER BY t.Id;";

            using var reader = cmd.ExecuteReader();

            Console.WriteLine("============================= Tables =============================");
            Console.WriteLine("ID | Name       | Type     | Status     | Assigned Staff");
            Console.WriteLine("===================================================================");

            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                string type = reader.IsDBNull(2) ? "Standard" : reader.GetString(2);
                string status = reader.IsDBNull(3) ? "Unoccupied" : reader.GetString(3);
                string staffName = reader.IsDBNull(4) ? "-" : reader.GetString(4);

                Console.WriteLine($"{id,2} | {name,-10} | {type,-8} | {status,-10} | {staffName}");
            }
            Console.WriteLine("===================================================================");
        }

        public static void ListAvailableTables()
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, Name, Type FROM Tables
                WHERE Status = 'Unoccupied'
                ORDER BY Id;";

            using var reader = cmd.ExecuteReader();

            Console.WriteLine("=== Available Tables ===");
            Console.WriteLine("ID | Name       | Type");
            Console.WriteLine("----------------------");

            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string name = reader.GetString(1);
                string type = reader.IsDBNull(2) ? "Standard" : reader.GetString(2);

                Console.WriteLine($"{id,2} | {name,-10} | {type}");
            }
            Console.WriteLine("========================");
        }

        public static void AddTable()
        {
            Console.Write("Enter Table Name (e.g., Table 1): ");
            string name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Name cannot be empty.");
                return;
            }

            bool isCorrect = false;
            string type = "";

            while (!isCorrect)
            {
                Console.WriteLine("Please Choose Table Type:");
                Console.WriteLine("[1]: Standard");
                Console.WriteLine("[2]: VIP");
                Console.WriteLine("[3]: Outdoor");
                Console.Write("Enter Table Type (1-3): ");
                string input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input))
                {
                    type = "Standard";
                    isCorrect = true;
                }
                else
                {
                    switch (input)
                    {
                        case "1":
                            type = "Standard";
                            isCorrect = true;
                            break;
                        case "2":
                            type = "VIP";
                            isCorrect = true;
                            break;
                        case "3":
                            type = "Outdoor";
                            isCorrect = true;
                            break;
                        default:
                            Console.WriteLine("Invalid input, please enter 1, 2, or 3.");
                            isCorrect = false;
                            break;
                    }
                }
            }

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Tables (Name, Type, Status) VALUES (@name, @type, 'Available');";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@type", type);

            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0 ? "Table added successfully." : "Failed to add table.");
        }

        public static void RemoveTable(int id)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Tables WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);

            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0 ? "Table removed." : "Table not found or remove failed.");
        }

        public static void ResetTable(int id)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            // Reset status to Unoccupied and remove staff assignment
            cmd.CommandText = "UPDATE Tables SET Status = 'Unoccupied', StaffId = NULL WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);

            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0 ? "Table reset successfully." : "Table not found or reset failed.");
        }

        public static void UpdateTableStatus(int id, string newStatus)
        {
            if (newStatus != "Occupied" && newStatus != "Unoccupied" && newStatus != "Broken")
            {
                Console.WriteLine("Invalid status. Allowed: Occupied, Unoccupied, Broken");
                return;
            }

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Tables SET Status = @status WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@status", newStatus);
            cmd.Parameters.AddWithValue("@id", id);

            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0 ? "Table status updated." : "Table not found or update failed.");
        }

        public static void AssignStaff(int tableId, int staffId)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();

            // Optional: Validate staffId exists first (not shown here)

            cmd.CommandText = "UPDATE Tables SET StaffId = @staffId WHERE Id = @tableId;";
            cmd.Parameters.AddWithValue("@staffId", staffId);
            cmd.Parameters.AddWithValue("@tableId", tableId);

            int rows = cmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0 ? "Staff assigned to table." : "Table or Staff not found.");
        }
        public static void UnassignStaff(int tableId)
        {
            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE Tables
                SET StaffId = NULL
                WHERE Id = @tableId;
            ";
            cmd.Parameters.AddWithValue("@tableId", tableId);

            int rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                Console.WriteLine($"Staff unassigned from table {tableId} successfully.");
            }
            else
            {
                Console.WriteLine($"Table with ID {tableId} not found or no staff assigned.");
            }
        }
        public static void UpdateTable(int id)
        {
            Console.WriteLine("Updating table info. Leave input empty to keep current value.");

            using var conn = DatabaseContext.GetConnection();
            conn.Open();

            // First fetch current data
            string currentName = "";
            string currentType = "";
            string currentStatus = "";
            int? currentStaffId = null;

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Name, Type, Status, StaffId FROM Tables WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    currentName = reader.GetString(0);
                    currentType = reader.IsDBNull(1) ? "Standard" : reader.GetString(1);
                    currentStatus = reader.IsDBNull(2) ? "Unoccupied" : reader.GetString(2);
                    currentStaffId = reader.IsDBNull(3) ? null : reader.GetInt32(3);
                }
                else
                {
                    Console.WriteLine("Table not found.");
                    return;
                }
            }

            Console.Write($"Name ({currentName}): ");
            string name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name)) name = currentName;

            Console.Write($"Type ({currentType}): ");
            string type = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(type)) type = currentType;

            Console.Write($"Status ({currentStatus}): ");
            string status = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(status)) status = currentStatus;

            Console.Write($"Staff ID ({(currentStaffId.HasValue ? currentStaffId.ToString() : "None")}): ");
            string staffInput = Console.ReadLine()?.Trim();
            int? staffId = currentStaffId;
            if (!string.IsNullOrEmpty(staffInput))
            {
                if (int.TryParse(staffInput, out int sid))
                    staffId = sid;
                else
                    Console.WriteLine("Invalid staff ID input. Keeping previous value.");
            }

            using var updateCmd = conn.CreateCommand();
            updateCmd.CommandText = @"
                UPDATE Tables
                SET Name = @name, Type = @type, Status = @status, StaffId = @staffId
                WHERE Id = @id;";
            updateCmd.Parameters.AddWithValue("@name", name);
            updateCmd.Parameters.AddWithValue("@type", type);
            updateCmd.Parameters.AddWithValue("@status", status);
            if (staffId.HasValue)
                updateCmd.Parameters.AddWithValue("@staffId", staffId.Value);
            else
                updateCmd.Parameters.AddWithValue("@staffId", DBNull.Value);
            updateCmd.Parameters.AddWithValue("@id", id);

            int rows = updateCmd.ExecuteNonQuery();
            Console.WriteLine(rows > 0 ? "Table updated successfully." : "Failed to update table.");
        }
    }
}
