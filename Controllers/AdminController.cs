using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileStore.Data;
using MobileStore.Models;
using MobileStore.ViewModels.Admin;

namespace MobileStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _users;

        public AdminController(AppDbContext db, UserManager<ApplicationUser> users)
        {
            _db = db;
            _users = users;
        }

        // ── Dashboard ──────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;
            var month = new DateTime(now.Year, now.Month, 1);

            // Top phones by sales
            var topPhones = await _db.OrderItems
                .Include(i => i.Phone)
                    .ThenInclude(p => p.Brand)
                .GroupBy(i => i.PhoneId)
                .Select(g => new TopPhoneItem
                {
                    Phone = g.First().Phone,
                    TotalSold = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.UnitPrice * i.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            // Monthly sales (last 6 months)
            var monthlySalesData = await _db.Orders
                .Where(o => o.Status != OrderStatus.Cancelled &&
                            o.OrderDate >= now.AddMonths(-6))
                .GroupBy(o => new
                {
                    Year = o.OrderDate.Year,
                    Month = o.OrderDate.Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(o => o.TotalAmount),
                    Orders = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var monthlySales = monthlySalesData
                .Select(g => new MonthlySalesItem
                {
                    Month = $"{g.Year}-{g.Month:D2}",
                    Revenue = g.Revenue,
                    Orders = g.Orders
                })
                .ToList();

            var vm = new DashboardViewModel
            {
                TotalOrders = await _db.Orders.CountAsync(),

                PendingOrders = await _db.Orders
                    .CountAsync(o => o.Status == OrderStatus.Pending),

                TotalRevenue = await _db.Orders
                    .Where(o => o.Status != OrderStatus.Cancelled)
                    .SumAsync(o => o.TotalAmount),

                MonthRevenue = await _db.Orders
                    .Where(o => o.Status != OrderStatus.Cancelled &&
                                o.OrderDate >= month)
                    .SumAsync(o => o.TotalAmount),

                TotalPhones = await _db.Phones.CountAsync(),

                LowStockCount = await _db.Phones
                    .CountAsync(p => p.Stock <= 5 && p.IsAvailable),

                TotalUsers = await _users.Users.CountAsync(),

                RecentOrders = await _db.Orders
                    .Include(o => o.User)
                    .Include(o => o.Items)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToListAsync(),

                TopPhones = topPhones,
                MonthlySales = monthlySales
            };

            return View(vm);
        }
        // ── Phone Management ───────────────────────────────────────────────
        public async Task<IActionResult> Phones(string? q, int page = 1)
        {
            const int PageSize = 20;
            var query = _db.Phones.Include(p => p.Brand).Include(p => p.Seller).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Name.Contains(q));

            int total = await query.CountAsync();
            var phones = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Query = q;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PageSize);

            return View(phones);
        }

        // ── Toggle Phone Visibility ────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var phone = await _db.Phones.FindAsync(id);
            if (phone != null)
            {
                phone.IsAvailable = !phone.IsAvailable;
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = phone == null
                ? "Phone not found."
                : $"Phone {(phone.IsAvailable ? "is now visible" : "has been hidden")}.";
            return RedirectToAction("Phones");
        }

        // ── Orders Management (read-only monitoring) ───────────────────────
        public async Task<IActionResult> Orders(string? status, int page = 1)
        {
            const int PageSize = 20;
            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items).ThenInclude(i => i.Phone).ThenInclude(p => p.Seller)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, out var s))
                query = query.Where(o => o.Status == s);

            int total = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PageSize);

            return View(orders);
        }

        // ── Block Seller ───────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(string id)
        {
            var user = await _users.FindByIdAsync(id);
            if (user != null)
            {
                user.IsBlocked = true;
                await _users.UpdateAsync(user);
                TempData["Success"] = $"{user.FullName} has been blocked.";
            }
            return RedirectToAction("Users");
        }

        // ── Unblock Seller ─────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(string id)
        {
            var user = await _users.FindByIdAsync(id);
            if (user != null)
            {
                user.IsBlocked = false;
                await _users.UpdateAsync(user);
                TempData["Success"] = $"{user.FullName} has been unblocked.";
            }
            return RedirectToAction("Users");
        }

        // ── Users ──────────────────────────────────────────────────────────
        public IActionResult Users()
        {
            var allUsers = _users.Users.OrderByDescending(u => u.CreatedAt).ToList();
            return View(allUsers);
        }

        // ── Manage Sellers ──────────────────────────────────────────────────
        public async Task<IActionResult> Sellers(string? q)
        {
            var query = _users.Users.Where(u => u.IsSeller).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(u =>
                    u.FullName.Contains(q) ||
                    u.Email!.Contains(q) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(q)) ||
                    (u.ShopAddress != null && u.ShopAddress.Contains(q)));
            }

            var sellers = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(sellers);
        }

        // ── View Seller Products ────────────────────────────────────────────
        public async Task<IActionResult> SellerProducts(string id)
        {
            var seller = await _users.FindByIdAsync(id);
            if (seller == null) return NotFound();

            var phones = await _db.Phones
                .Include(p => p.Brand)
                .Where(p => p.SellerId == id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Seller = seller;
            return View(phones);
        }

        // ── View Seller Orders ──────────────────────────────────────────────
        public async Task<IActionResult> SellerOrders(string id)
        {
            var seller = await _users.FindByIdAsync(id);
            if (seller == null) return NotFound();

            var orders = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.Items).ThenInclude(i => i.Phone)
                .Where(o => o.Items.Any(i => i.Phone.SellerId == id))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.Seller = seller;
            return View(orders);
        }
    }
}