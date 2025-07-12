namespace UrbanZenith.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public string OrderDate { get; set; } // Store as string for simplicity
        public string Status { get; set; }
    }
}