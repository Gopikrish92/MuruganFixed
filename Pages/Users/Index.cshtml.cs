using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MuruganRestaurant.Data;
using MuruganRestaurant.Models;

namespace MuruganRestaurant.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public List<User> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            Users = await _context.Users
                .OrderBy(u => u.Role).ThenBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostSaveUserAsync(
            int userId, string fullName, string username,
            string email, string password, string role, string isActive)
        {
            try
            {
                // Parse isActive from string because unchecked checkbox sends nothing
                bool active = isActive == "true" || isActive == "True" || isActive == "on";

                // Validate required fields
                if (string.IsNullOrWhiteSpace(fullName))
                    return new JsonResult(new { success = false, message = "Full Name is required" });
                if (string.IsNullOrWhiteSpace(username))
                    return new JsonResult(new { success = false, message = "Username is required" });
                if (string.IsNullOrWhiteSpace(email))
                    return new JsonResult(new { success = false, message = "Email is required" });
                if (string.IsNullOrWhiteSpace(role))
                    return new JsonResult(new { success = false, message = "Role is required" });

                // DB CHECK constraint only allows 'Admin' or 'User'
                var allowedRoles = new[] { "Admin", "User" };
                if (!allowedRoles.Contains(role))
                    return new JsonResult(new { success = false, message = $"Invalid role '{role}'. Must be Admin or User." });

                if (userId == 0)
                {
                    // New user — password required
                    if (string.IsNullOrWhiteSpace(password))
                        return new JsonResult(new { success = false, message = "Password is required for new users" });

                    // Check duplicate username
                    if (await _context.Users.AnyAsync(u => u.Username == username))
                        return new JsonResult(new { success = false, message = "Username already exists. Choose a different username." });

                    // Check duplicate email
                    if (await _context.Users.AnyAsync(u => u.Email == email))
                        return new JsonResult(new { success = false, message = "Email already exists. Use a different email." });

                    var newUser = new User
                    {
                        FullName = fullName.Trim(),
                        Username = username.Trim(),
                        Email = email.Trim(),
                        PasswordHash = password,
                        Role = role,
                        IsActive = active,
                        CreatedDate = DateTime.Now
                    };
                    _context.Users.Add(newUser);
                }
                else
                {
                    // Edit existing user
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                        return new JsonResult(new { success = false, message = "User not found" });

                    // Check email duplicate (excluding self)
                    if (await _context.Users.AnyAsync(u => u.Email == email.Trim() && u.UserId != userId))
                        return new JsonResult(new { success = false, message = "Email already used by another user." });

                    user.FullName = fullName.Trim();
                    user.Email = email.Trim();
                    user.Role = role;
                    user.IsActive = active;

                    // Only update password if provided
                    if (!string.IsNullOrWhiteSpace(password))
                        user.PasswordHash = password;
                }

                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            catch (DbUpdateException dbEx)
            {
                // Show the real inner exception message
                var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                return new JsonResult(new { success = false, message = $"Database error: {innerMsg}" });
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                return new JsonResult(new { success = false, message = innerMsg });
            }
        }

        public async Task<IActionResult> OnPostToggleUserAsync(int userId, string isActive)
        {
            try
            {
                bool active = isActive == "true" || isActive == "True";
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return new JsonResult(new { success = false, message = "User not found" });

                user.IsActive = active;
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return new JsonResult(new { success = false, message = "User not found" });

                // Prevent deleting yourself
                var currentUsername = User.Identity?.Name;
                if (user.Username == currentUsername)
                    return new JsonResult(new { success = false, message = "You cannot delete your own account." });

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }
}
