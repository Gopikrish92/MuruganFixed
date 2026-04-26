//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.EntityFrameworkCore;
//using MuruganRestaurant.Data;
//using MuruganRestaurant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MuruganRestaurant.Data;
using MuruganRestaurant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuruganRestaurant.Pages.Billing
{
    [Authorize]
    public class PrintModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PrintModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Bill Bill { get; set; }
        public List<BillItem> BillItems { get; set; }
        public string UserName { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Bill = await _context.Bills.FirstOrDefaultAsync(b => b.BillId == id);

            if (Bill == null)
            {
                return NotFound();
            }

            BillItems = await _context.BillItems
                .Include(bi => bi.FoodItem)
                .Where(bi => bi.BillId == id)
                .ToListAsync();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == Bill.CreatedBy);

            UserName = user?.FullName ?? user?.Username ?? "Unknown";

            return Page();
        }
    }
}