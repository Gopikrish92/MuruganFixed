using System.ComponentModel.DataAnnotations;

namespace MuruganRestaurant.Models
{
    public class FoodItem
    {
        [Key]
        public int FoodId { get; set; }

        [Required]
        public string FoodName { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public string ImagePath { get; set; }
        public bool IsAvailable { get; set; }
    }
}