using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MuruganRestaurant.Models
{
    public class Bill
    {
        [Key]
        public int BillId { get; set; }

        [Required]
        public string BillNumber { get; set; }

        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }

        [Required]
        public DateTime BillDate { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal SubTotal { get; set; }

        [DataType(DataType.Currency)]
        public decimal Tax { get; set; }

        [DataType(DataType.Currency)]
        public decimal Discount { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        public string PaymentMethod { get; set; }

        [ForeignKey("User")]
        public int CreatedBy { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<BillItem> BillItems { get; set; }
    }
}