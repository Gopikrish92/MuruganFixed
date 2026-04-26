using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MuruganRestaurant.Data;
using System.Security.Claims;

namespace MuruganRestaurant.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public LoginInputModel Login { get; set; }

        public string ErrorMessage { get; set; }

        public class LoginInputModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == Login.Username && u.IsActive);

            if (user != null && user.PasswordHash == Login.Password)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.UserId.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                if (user.Role == "Admin")
                    return RedirectToPage("/Dashboard/Index");
                    //return RedirectToPage("/Reports/Index");
                else
                    return RedirectToPage("/Billing/Create");
            }

            ErrorMessage = "Invalid username or password";
            return Page();
        }
    }
}