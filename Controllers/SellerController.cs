using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileStore.Data;
using MobileStore.Models;
using MobileStore.ViewModels;
using MobileStore.ViewModels.Admin;
using MobileStore.ViewModels.Orders;

namespace MobileStore.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;

        public SellerController(AppDbContext db, UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn)
        {
            _db = db;
            _users = users;
            _signIn = signIn;
        }

        // ── Dashboard ──────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (user.IsBlocked)
            {
                await _signIn.SignOutAsync();
                TempData["Error"] = "Your account has been blocked. Contact support.";
                return RedirectToAction("Login", "Account");
            }

            var myPhones = await _db.Phones
                .Where(p => p.SellerId == user.Id)
                .ToListAsync();

            ViewBag.TotalPhones = myPhones.Count;
            ViewBag.ActivePhones = myPhones.Count(p => p.IsAvailable);
            ViewBag.LowStockCount = myPhones.Count(p => p.Stock <= 5);

            return View();
        }

        // ── My Phones ──────────────────────────────────────────────────────
        public async Task<IActionResult> Phones(string? q, int page = 1)
        {
            const int PageSize = 20;
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var query = _db.Phones
                .Include(p => p.Brand)
                .Where(p => p.SellerId == user.Id)
                .AsQueryable();

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

        // ── Orders ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Orders()
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var items = await _db.OrderItems
                .Include(i => i.Order).ThenInclude(o => o.User)
                .Include(i => i.Phone)
                .Where(i => i.Phone.SellerId == user.Id)
                .OrderByDescending(i => i.Order.OrderDate)
                .ToListAsync();

            // Group by order and include all seller's items per order
            var grouped = items.GroupBy(i => i.OrderId).Select(g =>
            {
                var first = g.First();
                return new SellerOrderViewModel
                {
                    OrderId = first.OrderId,
                    OrderDate = first.Order.OrderDate,
                    Status = first.Order.Status,
                    TotalAmount = first.Order.TotalAmount,
                    CustomerName = first.Order.User.FullName,
                    CustomerEmail = first.Order.User.Email ?? "",
                    CustomerPhone = first.Order.User.PhoneNumber,
                    CustomerAddress = first.Order.ShippingAddress != null
                        ? $"{first.Order.ShippingAddress}, {first.Order.City}"
                        : null,
                    Items = g.Select(i => new OrderItemLine
                    {
                        PhoneName = i.Phone.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                };
            }).ToList();

            return View(grouped);
        }

        // ── Update Order Status (seller's own items only) ──────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var order = await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Phone)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();

            // Verify seller owns at least one item in this order
            bool hasItem = order.Items.Any(i => i.Phone.SellerId == user.Id);
            if (!hasItem) return Forbid();

            var previousStatus = order.Status;

            // Handle stock restore/cancel for seller's items only
            if (status == OrderStatus.Cancelled && previousStatus != OrderStatus.Cancelled)
            {
                foreach (var item in order.Items.Where(i => i.Phone.SellerId == user.Id))
                {
                    if (item.Phone != null)
                    {
                        item.Phone.Stock += item.Quantity;
                    }
                }
            }

            if (status != OrderStatus.Cancelled && previousStatus == OrderStatus.Cancelled)
            {
                foreach (var item in order.Items.Where(i => i.Phone.SellerId == user.Id))
                {
                    if (item.Phone != null)
                    {
                        item.Phone.Stock -= item.Quantity;
                    }
                }
            }

            order.Status = status;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Order #{orderId} marked as {status}.";
            return RedirectToAction("Orders");
        }

        // ── Create Phone ───────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CreatePhone()
        {
            var user = await _users.GetUserAsync(User);
            if (user == null || user.IsBlocked) return Forbid();

            return View(new PhoneFormViewModel
            {
                Brands = await _db.Brands.ToListAsync()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePhone(PhoneFormViewModel vm)
        {
            vm.Brands = await _db.Brands.ToListAsync();
            if (!ModelState.IsValid) return View(vm);

            var user = await _users.GetUserAsync(User);
            if (user == null || user.IsBlocked) return Forbid();

            var phone = new Phone
            {
                Name = vm.Name,
                Slug = await GenerateUniqueSlugAsync(vm.Name),
                BrandId = vm.BrandId,
                Price = vm.Price,
                OldPrice = vm.OldPrice,
                RAM = vm.RAM,
                Storage = vm.Storage,
                ScreenSize = vm.ScreenSize,
                Battery = vm.Battery,
                Processor = vm.Processor,
                Camera = vm.Camera,
                OS = vm.OS,
                Network = vm.Network,
                Color = vm.Color,
                Description = vm.Description,
                ImageUrl = vm.ImageUrl,
                Condition = vm.Condition,
                Stock = vm.Stock,
                IsAvailable = vm.IsAvailable,
                IsFeatured = vm.IsFeatured,
                SellerId = user.Id
            };

            _db.Phones.Add(phone);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"{phone.Name} added successfully!";
            return RedirectToAction("Phones");
        }

        // ── Edit Phone ─────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> EditPhone(int id)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null || user.IsBlocked) return Forbid();

            var phone = await _db.Phones.FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);
            if (phone == null) return NotFound();

            var vm = new PhoneFormViewModel
            {
                Id = phone.Id,
                Name = phone.Name,
                BrandId = phone.BrandId,
                Price = phone.Price,
                OldPrice = phone.OldPrice,
                RAM = phone.RAM,
                Storage = phone.Storage,
                ScreenSize = phone.ScreenSize,
                Battery = phone.Battery,
                Processor = phone.Processor,
                Camera = phone.Camera,
                OS = phone.OS,
                Network = phone.Network,
                Color = phone.Color,
                Description = phone.Description,
                ImageUrl = phone.ImageUrl,
                Condition = phone.Condition,
                Stock = phone.Stock,
                IsAvailable = phone.IsAvailable,
                IsFeatured = phone.IsFeatured,
                Brands = await _db.Brands.ToListAsync()
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPhone(PhoneFormViewModel vm)
        {
            vm.Brands = await _db.Brands.ToListAsync();
            if (!ModelState.IsValid) return View(vm);

            var user = await _users.GetUserAsync(User);
            if (user == null || user.IsBlocked) return Forbid();

            var phone = await _db.Phones.FirstOrDefaultAsync(p => p.Id == vm.Id && p.SellerId == user.Id);
            if (phone == null) return NotFound();

            string oldName = phone.Name;
            phone.Name = vm.Name;
            if (phone.Name != oldName)
                phone.Slug = await GenerateUniqueSlugAsync(vm.Name, phone.Id);
            phone.BrandId = vm.BrandId;
            phone.Price = vm.Price;
            phone.OldPrice = vm.OldPrice;
            phone.RAM = vm.RAM;
            phone.Storage = vm.Storage;
            phone.ScreenSize = vm.ScreenSize;
            phone.Battery = vm.Battery;
            phone.Processor = vm.Processor;
            phone.Camera = vm.Camera;
            phone.OS = vm.OS;
            phone.Network = vm.Network;
            phone.Color = vm.Color;
            phone.Description = vm.Description;
            phone.ImageUrl = vm.ImageUrl;
            phone.Condition = vm.Condition;
            phone.Stock = vm.Stock;
            phone.IsAvailable = vm.IsAvailable;
            phone.IsFeatured = vm.IsFeatured;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Phone updated.";
            return RedirectToAction("Phones");
        }

        // ── Toggle Phone Visibility ────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null || user.IsBlocked) return Forbid();

            var phone = await _db.Phones.FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);
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

        // ── Delete Phone ───────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhone(int id)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null || user.IsBlocked) return Forbid();

            var phone = await _db.Phones.FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);
            if (phone != null)
            {
                phone.IsAvailable = false;
                await _db.SaveChangesAsync();
            }

            TempData["Success"] = "Phone removed.";
            return RedirectToAction("Phones");
        }

        // ── Helpers ────────────────────────────────────────────────────────
        private static string GenerateSlug(string name)
        {
            return name.ToLower()
                .Replace(" ", "-")
                .Replace("/", "-")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(",", "")
                .Replace(".", "")
                .Trim('-');
        }

        private async Task<string> GenerateUniqueSlugAsync(string name, int? excludeId = null)
        {
            var baseSlug = GenerateSlug(name);
            if (string.IsNullOrWhiteSpace(baseSlug))
                baseSlug = "phone";

            var slug = baseSlug;
            int counter = 0;

            while (await _db.Phones.AnyAsync(p => p.Slug == slug && (!excludeId.HasValue || p.Id != excludeId.Value)))
            {
                counter++;
                slug = $"{baseSlug}-{counter}";
            }

            return slug;
        }
    }
}
