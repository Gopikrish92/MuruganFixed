using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using MuruganRestaurant.Data;
using MuruganRestaurant.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// Add Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

//// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Important: Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Razor Pages
app.MapRazorPages();

// Set default page to Login
app.MapGet("/", context =>
{
    context.Response.Redirect("/Account/Login");
    return Task.CompletedTask;
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();

//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using MuruganRestaurant.Data;
//using MuruganRestaurant.Services;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container
//builder.Services.AddRazorPages();

//// Configure DbContext
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// Add Authentication
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Account/Login";
//        options.AccessDeniedPath = "/Account/AccessDenied";
//        options.ExpireTimeSpan = TimeSpan.FromHours(8);
//        options.SlidingExpiration = true;
//    });

//// Add Email Service
//builder.Services.AddScoped<IEmailService, EmailService>();

//// Add HttpContextAccessor
//builder.Services.AddHttpContextAccessor();

//var app = builder.Build();

//// Configure pipeline
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();
//app.UseRouting();

//// Important: Add authentication and authorization
//app.UseAuthentication();
//app.UseAuthorization();

//// Map Razor Pages
//app.MapRazorPages();

//// Set default page to Login
//app.MapGet("/", context =>
//{
//    context.Response.Redirect("/Account/Login");
//    return Task.CompletedTask;
//});

//// Seed database
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    dbContext.Database.EnsureCreated();
//    SeedData.Initialize(dbContext);
//}

//app.Run();