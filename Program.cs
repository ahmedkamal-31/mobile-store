using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MobileStore.Data;
using MobileStore.Models;
using MobileStore.Services;

var builder = WebApplication.CreateBuilder(args);

// ── EF Core + Identity ─────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── App Services ───────────────────────────────────────────────────────────
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();

// ── MVC ────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Auth cookie paths ──────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath    = "/Account/Login";
    options.LogoutPath   = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// ── Seed roles + admin user ────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var db          = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Run pending migrations automatically
    //db.Database.Migrate();

    // Seed roles
    foreach (var role in new[] { "Admin", "Seller", "Customer" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Seed admin user
    const string adminEmail = "admin@mobilestore.eg";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            FullName = "Store Admin",
            UserName = adminEmail,
            Email    = adminEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(admin, "Admin@123");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}

// ── Middleware pipeline ────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
