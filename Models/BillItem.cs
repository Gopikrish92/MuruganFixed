using System.ComponentModel.DataAnnotations.Schema;

namespace MuruganRestaurant.Models
{
    public class BillItem
    {
        public int BillItemId { get; set; }

        [ForeignKey("Bill")]
        public int BillId { get; set; }
        public virtual Bill Bill { get; set; }

        [ForeignKey("FoodItem")]
        public int FoodId { get; set; }
        public virtual FoodItem FoodItem { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}