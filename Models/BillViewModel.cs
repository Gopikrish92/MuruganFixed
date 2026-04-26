namespace MuruganRestaurant.Models
{
    public class BillViewModel
    {
        public Bill Bill { get; set; }
        public List<BillItem> BillItems { get; set; }
        public List<CartItem> CartItems { get; set; }
    }

    public class CartItem
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; }
        public decimal Total => Price * Quantity;
    }
}