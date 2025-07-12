namespace UrbanZenith.Models
{
    public class Table
    {
        public int Id { get; set; }
        public string Name { get; set; }          // e.g. "Table 1", "VIP Room"
        public string Type { get; set; }          // e.g. "Standard", "VIP"
        public string Status { get; set; }        // e.g. "Occupied", "Unoccupied", "Broken"
        public int? StaffId { get; set; }         // nullable FK to Staff assigned
    }
}
