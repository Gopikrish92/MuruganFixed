using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MuruganRestaurant.Data;
using MuruganRestaurant.Models;

namespace MuruganRestaurant.Pages.FoodItems
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public IndexModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public List<FoodItem> FoodItems { get; set; } = new();

        public async Task OnGetAsync()
        {
            FoodItems = await _context.FoodItems
                .OrderBy(f => f.Category).ThenBy(f => f.FoodName)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostSaveFoodAsync(
            int foodId, string foodName, string category,
            decimal price, bool isAvailable,
            string existingImagePath, IFormFile? imageFile)
        {
            try
            {
                string imagePath = existingImagePath ?? "";

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Validate file size (2MB max)
                    if (imageFile.Length > 2 * 1024 * 1024)
                        return new JsonResult(new { success = false, message = "Image size must be under 2MB" });

                    // Validate file type
                    var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                    if (!allowedTypes.Contains(imageFile.ContentType))
                        return new JsonResult(new { success = false, message = "Only JPG, PNG, GIF, WEBP images allowed" });

                    // Save image to wwwroot/images/food/
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "food");
                    Directory.CreateDirectory(uploadsFolder);

                    var ext = Path.GetExtension(imageFile.FileName);
                    var fileName = $"food_{DateTime.Now:yyyyMMddHHmmssfff}{ext}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await imageFile.CopyToAsync(stream);

                    imagePath = $"/images/food/{fileName}";
                }

                if (foodId == 0)
                {
                    // Add new food item
                    var food = new FoodItem
                    {
                        FoodName = foodName,
                        Category = category,
                        Price = price,
                        IsAvailable = isAvailable,
                        ImagePath = imagePath
                    };
                    _context.FoodItems.Add(food);
                }
                else
                {
                    // Update existing
                    var food = await _context.FoodItems.FindAsync(foodId);
                    if (food == null)
                        return new JsonResult(new { success = false, message = "Food item not found" });

                    food.FoodName = foodName;
                    food.Category = category;
                    food.Price = price;
                    food.IsAvailable = isAvailable;
                    if (!string.IsNullOrEmpty(imagePath))
                        food.ImagePath = imagePath;
                }

                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostDeleteFoodAsync(int foodId)
        {
            try
            {
                var food = await _context.FoodItems.FindAsync(foodId);
                if (food == null)
                    return new JsonResult(new { success = false, message = "Item not found" });

                _context.FoodItems.Remove(food);
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
