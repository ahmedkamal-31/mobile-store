using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileStore.Data;
using MobileStore.Models;
using MobileStore.Services;
using MobileStore.ViewModels.Phones;
using MobileStore.ViewModels.Cart;
using MobileStore.ViewModels.Orders;

namespace MobileStore.Controllers
{
    public class PhonesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly RecommendationService _rec;
        private readonly CartService _cart;
        private readonly OrderService _orderService;
        private readonly UserManager<ApplicationUser> _users;

        public PhonesController(
            AppDbContext db,
            RecommendationService rec,
            CartService cart,
            OrderService orderService,
            UserManager<ApplicationUser> users)
        {
            _db = db;
            _rec = rec;
            _cart = cart;
            _orderService = orderService;
            _users = users;
        }

        // ── GET /Phones ───────────────────────────────────────────────
        public async Task<IActionResult> Index(
            int? brandId,
            PhoneCondition? condition,
            string? q,
            string sort = "newest",
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int page = 1)
        {
            const int PageSize = 12;

            var query = _db.Phones
                .Include(p => p.Brand)
                .Include(p => p.Seller)
                .Where(p => p.IsAvailable && (p.Seller == null || !p.Seller.IsBlocked))
                .AsQueryable();

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.Name.Contains(q) || p.Brand.Name.Contains(q));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (condition.HasValue)
                query = query.Where(p => p.Condition == condition.Value);

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            int total = await query.CountAsync();

            var phones = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var vm = new PhoneListViewModel
            {
                Phones = phones,
                Brands = await _db.Brands.ToListAsync(),
                FilterBrandId = brandId,
                FilterCondition = condition,
                SearchQuery = q,
                SortBy = sort,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)PageSize),
                TotalCount = total
            };

            return View(vm);
        }

        // ── GET /Phones/Details/{slug} ───────────────────────────────
        public async Task<IActionResult> Details(string id)
        {
            var phone = await _db.Phones
                .Include(p => p.Brand)
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Slug == id);

            if (phone == null) return NotFound();

            var similar = await _rec.GetSimilarAsync(phone.Id, 4);
            bool inCart = false;

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _users.GetUserAsync(User);
                if (user != null)
                {
                    inCart = await _db.CartItems
                        .AnyAsync(c => c.UserId == user.Id && c.PhoneId == phone.Id);
                }
            }

            var vm = new PhoneDetailViewModel
            {
                Phone = phone,
                SimilarPhones = similar,
                IsInCart = inCart
            };

            return View(vm);
        }

        // ── GET /Phones/Compare ─────────────────────────────────────
        public async Task<IActionResult> Compare(int? a, int? b)
        {
            var allPhones = await _db.Phones
                .Include(p => p.Brand)
                .Include(p => p.Seller)
                .Where(p => p.IsAvailable && (p.Seller == null || !p.Seller.IsBlocked))
                .OrderBy(p => p.Brand.Name).ThenBy(p => p.Name)
                .ToListAsync();

            var vm = new CompareViewModel
            {
                AllPhones = allPhones
            };

            if (a.HasValue)
                vm.PhoneA = allPhones.FirstOrDefault(p => p.Id == a.Value);

            if (b.HasValue)
                vm.PhoneB = allPhones.FirstOrDefault(p => p.Id == b.Value);

            return View(vm);
        }

        // ── GET /Phones/Recommendations ─────────────────────────────
        public async Task<IActionResult> Recommendations(int id)
        {
            var phone = await _db.Phones
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phone == null) return NotFound();

            var recommended = await _rec.GetSimilarAsync(id, 6);

            var vm = new RecommendationViewModel
            {
                BasePhone = phone,
                Recommended = recommended
            };

            return View(vm);
        }

        // ── POST /Phones/AddToCart ──────────────────────────────────
        [HttpPost, Authorize]
        public async Task<IActionResult> AddToCart(int phoneId, int quantity = 1, string? returnUrl = null)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (await _users.IsInRoleAsync(user, "Seller"))
            {
                TempData["Error"] = "Sellers cannot purchase products.";
                return returnUrl != null ? LocalRedirect(returnUrl) : RedirectToAction("Index");
            }

            bool ok = await _cart.AddToCartAsync(user.Id, phoneId, quantity);

            TempData[ok ? "Success" : "Error"] = ok
                ? "Added to cart!"
                : "Could not add to cart — check availability.";

            return returnUrl != null
                ? LocalRedirect(returnUrl)
                : RedirectToAction("Index");
        }

        // ── GET /Phones/Cart ────────────────────────────────────────
        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (await _users.IsInRoleAsync(user, "Seller"))
            {
                TempData["Error"] = "Sellers cannot purchase products.";
                return RedirectToAction("Index", "Seller");
            }

            var vm = await _cart.GetCartAsync(user.Id);
            return View(vm);
        }

        // ── POST /Phones/RemoveFromCart ─────────────────────────────
        [HttpPost, Authorize]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _cart.RemoveFromCartAsync(user.Id, cartItemId);
            return RedirectToAction("Cart");
        }

        // ── POST /Phones/UpdateCartItem ────────────────────────────
        [HttpPost, Authorize]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, int quantity)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();

            bool ok = await _cart.UpdateCartItemQuantityAsync(user.Id, cartItemId, quantity);
            if (!ok)
                TempData["Error"] = "Could not update quantity — check stock.";
            return RedirectToAction("Cart");
        }

        // ── GET /Phones/Checkout ────────────────────────────────────
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (await _users.IsInRoleAsync(user, "Seller"))
            {
                TempData["Error"] = "Sellers cannot purchase products.";
                return RedirectToAction("Index", "Seller");
            }

            var cart = await _cart.GetCartAsync(user.Id);
            if (!cart.Items.Any()) return RedirectToAction("Cart");

            var vm = new CheckoutViewModel
            {
                Phone = user.PhoneNumber ?? "",
                Cart = cart
            };

            return View(vm);
        }

        // ── POST /Phones/Checkout ───────────────────────────────────
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel vm)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (await _users.IsInRoleAsync(user, "Seller"))
            {
                TempData["Error"] = "Sellers cannot purchase products.";
                return RedirectToAction("Index", "Seller");
            }

            vm.Cart = await _cart.GetCartAsync(user.Id);

            if (!ModelState.IsValid)
                return View(vm);

            var order = await _orderService.PlaceOrderAsync(user.Id, vm);

            if (order == null)
            {
                ModelState.AddModelError("", "Order could not be placed.");
                return View(vm);
            }

            return RedirectToAction("OrderSuccess", "Orders", new { id = order.Id });
        }
    }
}