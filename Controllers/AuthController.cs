using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Models;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSI402_Project_final.Controllers
{
    public class AuthController : Controller
    {
        private readonly PetShopRepository _repo;

        public AuthController(PetShopRepository repo)
        {
            _repo = repo;
        }

        // หน้า Login (GET)
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToRoleHome(User.FindFirstValue(ClaimTypes.Role));
            }

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.AllDemoUsers = GetDemoRoleUsers();
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        private List<SystemUser> GetDemoRoleUsers()
        {
            var systemUsernames = new[]
            {
                "owner", "admin", "marketing", "inventory", "order",
                "shipping", "accounting", "support", "customer"
            };
            var allUsers = _repo.Users;
            var prioritized = allUsers
                .Where(u => systemUsernames.Contains(u.Username.ToLowerInvariant()))
                .OrderByDescending(u => u.Role)
                .ToList();
            return prioritized.Any() ? prioritized : allUsers.Take(10).ToList();
        }

        // การประมวลผล Login (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            ViewBag.AllDemoUsers = GetDemoRoleUsers();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _repo.ValidateUser(model.Username, model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง หรือบัญชีถูกระงับ");
                return View(model);
            }

            await SignInUserAsync(user, model.RememberMe);
            _repo.MergeGuestCartToUser(user.Id);
            _repo.LogAudit("LOGIN", "user", user.Id.ToString(), $"เข้าสู่ระบบด้วย Username '{user.Username}'", user);

            TempData["SuccessMessage"] = $"ยินดีต้อนรับคุณ {user.FullName} เข้าสู่ระบบในฐานะ {user.Role}";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToRoleHome(user.Role.ToString());
        }

        // Quick Demo Login สำหรับทดสอบคลิกเดียว
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickLogin(string username)
        {
            var user = _repo.Users.FirstOrDefault(u =>
                u.Username.Equals(username, System.StringComparison.OrdinalIgnoreCase) && u.IsActive);
            if (user == null) return RedirectToAction(nameof(Login));

            await SignInUserAsync(user, isPersistent: true);
            _repo.MergeGuestCartToUser(user.Id);
            _repo.LogAudit("QUICK_LOGIN", "user", user.Id.ToString(), $"Quick Login '{user.Username}'", user);

            TempData["SuccessMessage"] = $"เข้าสู่ระบบสำเร็จ: {user.FullName} ({user.Role})";
            return RedirectToRoleHome(user.Role.ToString());
        }

        // หน้า Register (GET)
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterViewModel());
        }

        // การประมวลผล Register (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var newUser = _repo.RegisterCustomer(model);
            if (newUser == null)
            {
                ModelState.AddModelError(string.Empty, "ชื่อผู้ใช้หรืออีเมลนี้มีอยู่ในระบบแล้ว กรุณาใช้อันอื่น");
                return View(model);
            }

            await SignInUserAsync(newUser, isPersistent: true);
            _repo.MergeGuestCartToUser(newUser.Id);
            TempData["SuccessMessage"] = $"สมัครสมาชิกสำเร็จ! ยินดีต้อนรับคุณ {newUser.FullName}";

            return RedirectToAction("Index", "Home");
        }

        // Logout
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                _repo.LogAudit("LOGOUT", "user", User.FindFirstValue(ClaimTypes.NameIdentifier), "ออกจากระบบ");
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "คุณได้ออกจากระบบเรียบร้อยแล้ว";
            return RedirectToAction("Index", "Home");
        }

        // หน้า Access Denied เมื่อไม่มีสิทธิ์
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInUserAsync(SystemUser user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Phone", user.Phone)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = System.DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        // Helper: นำทางไปยังหน้าหลักของแต่ละ Role
        private IActionResult RedirectToRoleHome(string? role)
        {
            return role switch
            {
                nameof(UserRole.InventoryStaff) => RedirectToAction("Index", "Inventory"),
                nameof(UserRole.OrderStaff) => RedirectToAction("Index", "Order"),
                nameof(UserRole.ShippingStaff) => RedirectToAction("Index", "Shipping"),
                nameof(UserRole.MarketingStaff) => RedirectToAction("Index", "Marketing"),
                nameof(UserRole.AccountingStaff) => RedirectToAction("Index", "Accounting"),
                nameof(UserRole.ITSupport) => RedirectToAction("Index", "Support"),
                nameof(UserRole.Admin) => RedirectToAction("Index", "Admin"),
                nameof(UserRole.Owner) => RedirectToAction("Index", "Owner"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}
