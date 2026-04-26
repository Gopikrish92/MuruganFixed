using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MuruganRestaurant.Data;
using MuruganRestaurant.Models;

namespace MuruganRestaurant.Pages.Dashboard
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        // Today
        public int TodayBillsCount { get; set; }
        public decimal TodayRevenue { get; set; }
        // This month
        public int MonthBillsCount { get; set; }
        public decimal MonthRevenue { get; set; }
        // All time
        public int TotalBillsCount { get; set; }
        public decimal TotalRevenue { get; set; }
        // Others
        public int TotalFoodItems { get; set; }
        public int TotalUsers { get; set; }
        public List<Bill> RecentBills { get; set; }
        // Payment breakdown
        public decimal CashRevenue { get; set; }
        public decimal CardRevenue { get; set; }
        public decimal UpiRevenue { get; set; }

        public async Task OnGetAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var allBills = await _context.Bills.ToListAsync();

            // Today
            var todayBills = allBills.Where(b => b.BillDate >= today && b.BillDate < tomorrow).ToList();
            TodayBillsCount = todayBills.Count;
            TodayRevenue = todayBills.Sum(b => b.TotalAmount);

            // This month
            var monthBills = allBills.Where(b => b.BillDate >= monthStart).ToList();
            MonthBillsCount = monthBills.Count;
            MonthRevenue = monthBills.Sum(b => b.TotalAmount);

            // All time
            TotalBillsCount = allBills.Count;
            TotalRevenue = allBills.Sum(b => b.TotalAmount);

            // Payment breakdown (all time)
            CashRevenue = allBills.Where(b => b.PaymentMethod == "Cash").Sum(b => b.TotalAmount);
            CardRevenue = allBills.Where(b => b.PaymentMethod == "Card").Sum(b => b.TotalAmount);
            UpiRevenue  = allBills.Where(b => b.PaymentMethod == "UPI").Sum(b => b.TotalAmount);

            TotalFoodItems = await _context.FoodItems.CountAsync(f => f.IsAvailable);
            TotalUsers = await _context.Users.CountAsync(u => u.IsActive);

            RecentBills = await _context.Bills
                .OrderByDescending(b => b.BillDate)
                .Take(8)
                .ToListAsync();
        }
    }
}
