using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileStore.Data;
using MobileStore.Services;

namespace MobileStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly CartService  _cart;

        public HomeController(AppDbContext db, CartService cart)
        {
            _db   = db;
            _cart = cart;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Featured = await _db.Phones.Include(p => p.Brand).Include(p => p.Seller)
                .Where(p => p.IsFeatured && p.IsAvailable && (p.Seller == null || !p.Seller.IsBlocked)).Take(6).ToListAsync();
            ViewBag.Brands   = await _db.Brands.ToListAsync();
            ViewBag.Latest   = await _db.Phones.Include(p => p.Brand).Include(p => p.Seller)
                .Where(p => p.IsAvailable && (p.Seller == null || !p.Seller.IsBlocked)).OrderByDescending(p => p.CreatedAt).Take(8).ToListAsync();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _db.Users.Where(u => u.UserName == User.Identity.Name).Select(u => u.Id).FirstOrDefault();
                if (userId != null)
                    ViewBag.CartCount = await _cart.GetCartCountAsync(userId);
            }

            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new MobileStore.Models.ErrorViewModel
            { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
