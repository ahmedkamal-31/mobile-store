using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MobileStore.Data;
using MobileStore.Models;

namespace MobileStore.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _users;

        public OrdersController(AppDbContext db, UserManager<ApplicationUser> users)
        {
            _db    = db;
            _users = users;
        }

        // GET /Orders
        public async Task<IActionResult> Index()
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var orders = await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Phone)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET /Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var order = await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Phone).ThenInclude(p => p.Brand)
                .Include(o => o.Items).ThenInclude(i => i.Phone).ThenInclude(p => p.Seller)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            bool isAdmin = User.IsInRole("Admin");
            if (!isAdmin && order.UserId != user.Id) return Forbid();

            return View(order);
        }

        // GET /Orders/OrderSuccess/5
        public async Task<IActionResult> OrderSuccess(int id)
        {
            var order = await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Phone)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // POST /Orders/Cancel/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var order = await _db.Orders
                .Include(o => o.Items).ThenInclude(i => i.Phone)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // Owner can cancel their own pending/confirmed orders; Admin can cancel any
            bool isOwner = order.UserId == user.Id;
            bool isAdmin = User.IsInRole("Admin");
            if (!isOwner && !isAdmin) return Forbid();

            if (order.Status == OrderStatus.Cancelled)
            {
                TempData["Error"] = "This order is already cancelled.";
            }
            else if (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Confirmed)
            {
                order.Status = OrderStatus.Cancelled;

                // Restore stock for each item
                foreach (var item in order.Items)
                {
                    if (item.Phone != null)
                    {
                        item.Phone.Stock += item.Quantity;
                    }
                }

                await _db.SaveChangesAsync();
                TempData["Success"] = "Order cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "This order cannot be cancelled at its current status.";
            }

            return RedirectToAction("Details", new { id });
        }
    }
}
