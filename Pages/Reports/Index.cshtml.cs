using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MuruganRestaurant.Data;
using MuruganRestaurant.Models;
using MuruganRestaurant.Services;
using System.Text;

namespace MuruganRestaurant.Pages.Reports
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public IndexModel(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public List<Bill> Bills { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostGenerateReportAsync(DateTime fromDate, DateTime toDate)
        {
            FromDate = fromDate;
            ToDate = toDate.AddDays(1);

            Bills = await _context.Bills
                .Include(b => b.User)
                .Where(b => b.BillDate >= fromDate && b.BillDate <= ToDate)
                .OrderByDescending(b => b.BillDate)
                .ToListAsync();

            return Content(GenerateReportHtml(), "text/html");
        }

        // ✅ Updated: accepts ToEmail from request body
        public async Task<IActionResult> OnPostSendEmailAsync([FromBody] DateRangeDto dateRange)
        {
            try
            {
                if (dateRange == null || dateRange.FromDate == default || dateRange.ToDate == default)
                    return new JsonResult(new { success = false, message = "Invalid date range" });

                // ✅ Use provided email or fallback to logged-in user's email
                var recipientEmail = dateRange.ToEmail;
                if (string.IsNullOrWhiteSpace(recipientEmail))
                {
                    recipientEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                    if (string.IsNullOrEmpty(recipientEmail))
                        return new JsonResult(new { success = false, message = "No email address provided." });
                }

                // Basic email validation
                if (!recipientEmail.Contains("@") || !recipientEmail.Contains("."))
                    return new JsonResult(new { success = false, message = "Invalid email address format." });

                var toDate = dateRange.ToDate.AddDays(1);
                var bills = await _context.Bills.Include(b => b.User)
                    .Where(b => b.BillDate >= dateRange.FromDate && b.BillDate <= toDate)
                    .ToListAsync();

                if (!bills.Any())
                    return new JsonResult(new { success = false, message = "No bills found for the selected date range." });

                await _emailService.SendEmailWithAttachmentAsync(
                    recipientEmail,
                    $"Bills Report - {dateRange.FromDate:dd-MM-yyyy} to {dateRange.ToDate:dd-MM-yyyy}",
                    $"Bills report attached.\n\nFrom: {dateRange.FromDate:dd-MM-yyyy}\nTo: {dateRange.ToDate:dd-MM-yyyy}\nTotal Bills: {bills.Count}\nTotal Amount: Rs.{bills.Sum(b => b.TotalAmount):N2}",
                    GenerateCsv(bills));

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        private string GenerateReportHtml()
        {
            if (Bills == null || !Bills.Any())
                return "<div class='alert alert-info m-3'><i class='fas fa-info-circle me-2'></i>No bills found for the selected date range.</div>";

            var sb = new StringBuilder();

            sb.AppendLine("<div class='row g-3 p-3 border-bottom'>");
            sb.AppendLine($"<div class='col-auto'><span class='badge bg-warning text-dark fs-6'>{Bills.Count} Bills</span></div>");
            sb.AppendLine($"<div class='col-auto'><span class='badge bg-success fs-6'>Total: Rs.{Bills.Sum(b => b.TotalAmount):N2}</span></div>");
            sb.AppendLine($"<div class='col-auto'><span class='badge bg-secondary fs-6'>{FromDate:dd-MM-yyyy} to {ToDate?.AddDays(-1):dd-MM-yyyy}</span></div>");
            sb.AppendLine("</div>");
            sb.AppendLine("<div class='table-responsive'>");
            sb.AppendLine("<table class='table table-hover mb-0' id='reportTable'>");
            sb.AppendLine("<thead class='table-dark'><tr><th>#</th><th>Bill No</th><th>Date</th><th>Customer</th><th>Sub Total</th><th>Tax</th><th>Discount</th><th>Total</th><th>Payment</th><th>By</th><th>Print</th></tr></thead><tbody>");

            int i = 1;
            foreach (var bill in Bills)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td class='text-muted'>{i++}</td>");
                sb.AppendLine($"<td><small class='fw-bold'>{bill.BillNumber}</small></td>");
                sb.AppendLine($"<td><small>{bill.BillDate:dd-MM-yyyy HH:mm}</small></td>");
                sb.AppendLine($"<td>{bill.CustomerName}</td>");
                sb.AppendLine($"<td>Rs.{bill.SubTotal:N0}</td>");
                sb.AppendLine($"<td>Rs.{bill.Tax:N0}</td>");
                sb.AppendLine($"<td>Rs.{bill.Discount:N0}</td>");
                sb.AppendLine($"<td class='fw-bold text-success'>Rs.{bill.TotalAmount:N0}</td>");
                sb.AppendLine($"<td><span class='badge bg-light text-dark border'>{bill.PaymentMethod}</span></td>");
                sb.AppendLine($"<td><small>{bill.User?.Username}</small></td>");
                sb.AppendLine($"<td><button class='btn btn-sm btn-outline-warning' onclick='openBillPrint({bill.BillId})' title='Print'><i class='fas fa-print'></i></button></td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody><tfoot class='table-warning'><tr>");
            sb.AppendLine("<td colspan='4' class='text-end fw-bold'>Totals:</td>");
            sb.AppendLine($"<td class='fw-bold'>Rs.{Bills.Sum(b => b.SubTotal):N0}</td>");
            sb.AppendLine($"<td class='fw-bold'>Rs.{Bills.Sum(b => b.Tax):N0}</td>");
            sb.AppendLine($"<td class='fw-bold'>Rs.{Bills.Sum(b => b.Discount):N0}</td>");
            sb.AppendLine($"<td class='fw-bold text-success'>Rs.{Bills.Sum(b => b.TotalAmount):N0}</td>");
            sb.AppendLine("<td colspan='3'></td>");
            sb.AppendLine("</tr></tfoot></table></div>");
            return sb.ToString();
        }

        private byte[] GenerateCsv(List<Bill> bills)
        {
            using var ms = new MemoryStream();
            using var w = new StreamWriter(ms, Encoding.UTF8);
            w.WriteLine("Bill No,Date,Customer,Phone,SubTotal,Tax,Discount,Total,Payment,Created By");
            foreach (var b in bills)
                w.WriteLine($"\"{b.BillNumber}\",{b.BillDate:dd-MM-yyyy HH:mm},\"{b.CustomerName}\",\"{b.CustomerPhone}\",{b.SubTotal},{b.Tax},{b.Discount},{b.TotalAmount},{b.PaymentMethod},{b.User?.Username}");
            w.Flush();
            return ms.ToArray();
        }
    }

    public class DateRangeDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? ToEmail { get; set; }  // ✅ New field for recipient email
    }
}
