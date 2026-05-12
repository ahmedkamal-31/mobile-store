using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MobileStore.Models;
using MobileStore.ViewModels;
using MobileStore.ViewModels.Auth;

namespace MobileStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly SignInManager<ApplicationUser> _signIn;

        public AccountController(
            UserManager<ApplicationUser> users,
            SignInManager<ApplicationUser> signIn)
        {
            _users = users;
            _signIn = signIn;
        }

        // ── Register ───────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Register() =>
            User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Home") : View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new ApplicationUser
            {
                FullName = vm.FullName,
                UserName = vm.Email,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                IsSeller = vm.Role == "Seller",
                ShopAddress = vm.Role == "Seller" ? vm.ShopAddress : null
            };

            var result = await _users.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                string role = vm.Role == "Seller" ? "Seller" : "Customer";
                await _users.AddToRoleAsync(user, role);
                await _signIn.SignInAsync(user, isPersistent: false);
                TempData["Success"] = $"Welcome, {user.FullName}!";
                return role == "Seller"
                    ? RedirectToAction("Index", "Seller")
                    : RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(vm);
        }

        // ── Login ──────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return User.Identity?.IsAuthenticated == true
                ? RedirectToAction("Index", "Home")
                : View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _signIn.PasswordSignInAsync(
                vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _users.FindByEmailAsync(vm.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View(vm);
                }

                if (user.IsBlocked)
                {
                    await _signIn.SignOutAsync();
                    ModelState.AddModelError("", "Your account has been blocked. Contact support.");
                    return View(vm);
                }

                TempData["Success"] = "Welcome back!";

                if (returnUrl != null && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                if (await _users.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Admin");
                if (await _users.IsInRoleAsync(user, "Seller"))
                    return RedirectToAction("Index", "Seller");

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
                ModelState.AddModelError("", "Account is locked. Try again later.");
            else
                ModelState.AddModelError("", "Invalid email or password.");

            return View(vm);
        }

        // ── Logout ─────────────────────────────────────────────────────────
        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ── Profile ────────────────────────────────────────────────────────
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return NotFound();
            return View(user);
        }

        public IActionResult AccessDenied() => View();
    }
}