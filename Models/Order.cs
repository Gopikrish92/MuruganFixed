namespace MuruganRestaurant.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";

        // Navigation property
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}