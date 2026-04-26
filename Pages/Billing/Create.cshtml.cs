using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MuruganRestaurant.Models;
using Microsoft.EntityFrameworkCore;
using MuruganRestaurant.Data;

namespace MuruganRestaurant.Pages.Billing
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<FoodItem> FoodItems { get; set; } = new List<FoodItem>();

        [BindProperty]
        public Order Order { get; set; }

        public async Task OnGetAsync()
        {
            FoodItems = await _context.FoodItems
                .Where(f => f.IsAvailable)
                .OrderBy(f => f.Category).ThenBy(f => f.FoodName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostGenerateBillAsync([FromBody] BillRequest request)
        {
            try
            {
                string billNumber = GenerateBillNumber();
                decimal subTotal = request.Items.Sum(i => i.Price * i.Quantity);
                decimal tax = subTotal * 0.05m;
                decimal total = subTotal + tax - request.Discount;

                var bill = new Bill
                {
                    BillNumber = billNumber,
                    CustomerName = string.IsNullOrEmpty(request.CustomerName) ? "Walk-in" : request.CustomerName,
                    CustomerPhone = request.CustomerPhone,
                    BillDate = DateTime.Now,
                    SubTotal = subTotal,
                    Tax = tax,
                    Discount = request.Discount,
                    TotalAmount = total,
                    PaymentMethod = request.PaymentMethod,
                    CreatedBy = int.Parse(User.FindFirst("UserId")?.Value ?? "0")
                };

                _context.Bills.Add(bill);
                await _context.SaveChangesAsync();

                foreach (var item in request.Items)
                {
                    _context.BillItems.Add(new BillItem
                    {
                        BillId = bill.BillId,
                        FoodId = item.FoodId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        TotalPrice = item.Price * item.Quantity
                    });
                }

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true, billId = bill.BillId, billNumber = billNumber });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        // Handler to find a bill by bill number for reprint
        public async Task<IActionResult> OnGetGetBillIdAsync(string billNumber)
        {
            var bill = await _context.Bills.FirstOrDefaultAsync(b => b.BillNumber == billNumber);
            if (bill == null)
                return new JsonResult(new { success = false, message = "Bill not found" });
            return new JsonResult(new { success = true, billId = bill.BillId });
        }

        // Handler to get recent bills for quick reprint
        public async Task<IActionResult> OnGetRecentBillsAsync()
        {
            var bills = await _context.Bills
                .OrderByDescending(b => b.BillDate)
                .Take(5)
                .Select(b => new { b.BillId, b.BillNumber })
                .ToListAsync();
            return new JsonResult(new { bills });
        }

        private string GenerateBillNumber()
        {
            string date = DateTime.Now.ToString("yyyyMMdd");
            string random = DateTime.Now.ToString("HHmmssfff");
            return $"BILL-{date}-{random}";
        }

        public class BillRequest
        {
            public string CustomerName { get; set; }
            public string CustomerPhone { get; set; }
            public string PaymentMethod { get; set; }
            public decimal Discount { get; set; }
            public List<CartItemRequest> Items { get; set; }
        }

        public class CartItemRequest
        {
            public int FoodId { get; set; }
            public string FoodName { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public string ImagePath { get; set; }
        }
    }
}
