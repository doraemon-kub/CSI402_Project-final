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

        // หน้า Login (GET) - แสดงหน้าเข้าสู่ระบบเสมอเพื่อให้แต่ละแท็บล็อกอินแยกบทบาทได้อย่างอิสระ
        [HttpGet]
        public IActionResult Login(
            string? returnUrl = null,
            bool suspended = false,
            string? u = null,
            string? reason = null,
            string? until = null)
        {
            if (suspended || !string.IsNullOrEmpty(u))
            {
                var dbUser = !string.IsNullOrEmpty(u) ? _repo.FindUserByUsernameOrEmail(u) : null;
                if (dbUser != null && !dbUser.IsActive)
                {
                    ViewBag.SuspendedUser = dbUser.Username;
                    ViewBag.SuspendedReason = dbUser.SuspensionReason ?? reason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ";
                    ViewBag.SuspendedUntil = dbUser.SuspensionDisplayUntil;
                }
                else if (suspended)
                {
                    ViewBag.SuspendedUser = u ?? "บัญชีของคุณ";
                    ViewBag.SuspendedReason = reason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ";
                    ViewBag.SuspendedUntil = until ?? "ระงับถาวร";
                }
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
            foreach (var u in allUsers)
            {
                _repo.CheckAndRefreshUserSuspension(u);
            }
            allUsers = _repo.Users;
            var prioritized = allUsers
                .Where(u => systemUsernames.Contains(u.Username.ToLowerInvariant()))
                .OrderByDescending(u => u.Role)
                .ToList();
            return prioritized.Any() ? prioritized : allUsers.Take(10).ToList();
        }

        // การประมวลผล Login (POST)
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            ViewBag.AllDemoUsers = GetDemoRoleUsers();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var candidate = _repo.FindUserByUsernameOrEmail(model.Username);
            if (candidate != null && candidate.Password == model.Password && !candidate.IsActive)
            {
                ViewBag.SuspendedUser = candidate.Username;
                ViewBag.SuspendedReason = candidate.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ";
                ViewBag.SuspendedUntil = candidate.SuspensionDisplayUntil;
                ModelState.AddModelError(string.Empty, $"บัญชี '{candidate.Username}' ถูกระงับการใช้งาน ({candidate.SuspensionDisplayUntil}) เนื่องจาก: {ViewBag.SuspendedReason}");
                return View(model);
            }

            var user = _repo.ValidateUser(model.Username, model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง");
                return View(model);
            }

            await SignInUserAsync(user, model.RememberMe);
            _repo.MergeGuestCartToUser(user.Id);
            _repo.LogAudit("LOGIN", "user", user.Id.ToString(), $"เข้าสู่ระบบด้วย Username '{user.Username}'", user);

            TempData["SuccessMessage"] = FormatWelcomeMessage(user.FullName);
            TempData["SuccessTargetRole"] = user.Role.ToString();
            TempData["SuccessTargetUser"] = user.Username;

            return RedirectToRoleHome(user.Role.ToString(), user.Username);
        }

        // Quick Demo Login สำหรับทดสอบคลิกเดียว (ตรวจสอบสถานะการระงับบัญชีก่อนอนุญาตให้เข้าสู่ระบบ)
        [HttpGet]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> QuickLogin(string? username = null, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(username)) return RedirectToAction(nameof(Login));

            var user = _repo.FindUserByUsernameOrEmail(username);
            if (user == null)
            {
                var info = DemoUserHelper.GetDemoUserInfo(username);
                user = _repo.FindUserByUsernameOrEmail(info.Username);
            }

            if (user == null) return RedirectToAction(nameof(Login));

            // หากบัญชีถูกระงับอยู่ ห้ามเข้าสู่ระบบเด็ดขาด และแสดงเหตุผลพร้อมระยะเวลาที่ถูกระงับ
            if (!user.IsActive)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login), new
                {
                    suspended = true,
                    u = user.Username,
                    reason = user.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ",
                    until = user.SuspensionDisplayUntil
                });
            }

            await SignInUserAsync(user, isPersistent: true);
            _repo.MergeGuestCartToUser(user.Id);
            _repo.LogAudit("QUICK_LOGIN", "user", user.Id.ToString(), $"Quick Login '{user.Username}'", user);

            TempData["SuccessMessage"] = FormatWelcomeMessage(user.FullName);
            TempData["SuccessTargetRole"] = user.Role.ToString();
            TempData["SuccessTargetUser"] = user.Username;

            return RedirectToRoleHome(user.Role.ToString(), user.Username);
        }

        // Multi-Role Switcher: สลับบทบาทผู้ใช้แบบเรียลไทม์ (หากบัญชีของบทบาทนั้นถูกระงับ จะไม่อนุญาตให้เข้าใช้งาน)
        [HttpGet]
        public async Task<IActionResult> SwitchRole(string role, string? returnUrl = null)
        {
            var info = DemoUserHelper.GetDemoUserInfo(role);
            var user = !string.IsNullOrEmpty(info.Username)
                ? _repo.FindUserByUsernameOrEmail(info.Username)
                : null;

            if (user != null && !user.IsActive)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login), new
                {
                    suspended = true,
                    u = user.Username,
                    reason = user.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ",
                    until = user.SuspensionDisplayUntil
                });
            }

            if (user != null)
            {
                await SignInUserAsync(user, isPersistent: true);
                _repo.MergeGuestCartToUser(user.Id);
                _repo.LogAudit("ROLE_SWITCH", "user", user.Id.ToString(), $"สลับบทบาทเป็น '{user.Username}' ({info.RoleName})", user);
                TempData["SuccessMessage"] = FormatWelcomeMessage(user.FullName);
                TempData["SuccessTargetRole"] = info.RoleName;
                TempData["SuccessTargetUser"] = user.Username;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToRoleHome(info.RoleName);
        }

        // หน้า Register (GET) - แสดงหน้าสมัครสมาชิกเสมอโดยไม่เด้งกลับหน้าหลักแม้แท็บอื่นจะล็อกอินอยู่
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // การประมวลผล Register (POST)
        [HttpPost]
        [IgnoreAntiforgeryToken]
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
            TempData["SuccessTargetRole"] = "Customer";
            TempData["SuccessTargetUser"] = newUser.Username;

            return RedirectToAction("Index", "Home", new { role = newUser.Username });
        }

        // Logout
        [HttpPost]
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Logout()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                _repo.LogAudit("LOGOUT", "user", User.FindFirstValue(ClaimTypes.NameIdentifier), "ออกจากระบบ");
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "คุณได้ออกจากระบบเรียบร้อยแล้ว";
            TempData["SuccessTargetRole"] = "Guest";
            TempData["SuccessTargetUser"] = "";
            return RedirectToAction("Index", "Home", new { role = "Guest" });
        }

        // หน้า Access Denied เมื่อไม่มีสิทธิ์
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private static string CleanName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(fullName, @"\s*\([^)]*\)", "").Trim();
        }

        private static string FormatWelcomeMessage(string? fullName)
        {
            var clean = CleanName(fullName);
            if (string.IsNullOrWhiteSpace(clean))
            {
                return "ยินดีต้อนรับเข้าสู่ระบบ";
            }

            if (clean.StartsWith("คุณ "))
            {
                clean = clean.Substring(4).Trim();
            }
            else if (clean.StartsWith("คุณ"))
            {
                clean = clean.Substring(3).Trim();
            }

            return string.IsNullOrWhiteSpace(clean) ? "ยินดีต้อนรับเข้าสู่ระบบ" : $"ยินดีต้อนรับคุณ {clean}";
        }

        private async Task SignInUserAsync(SystemUser user, bool isPersistent)
        {
            var cleanFullName = CleanName(user.FullName);
            if (string.IsNullOrWhiteSpace(cleanFullName)) cleanFullName = user.Username;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("FullName", cleanFullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Phone", user.Phone)
            };

            // Multi-Role Support: สำหรับ Owner และ Admin ให้สิทธิ์เข้าถึงทุก Role พร้อมกันได้ เพื่อให้เปิดหลายแท็บ/หลายหน้าต่างทดสอบได้ทันที
            if (user.Role == UserRole.Owner || user.Role == UserRole.Admin)
            {
                foreach (UserRole r in Enum.GetValues(typeof(UserRole)))
                {
                    if (r != user.Role)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, r.ToString()));
                    }
                }
            }

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

        // Helper: นำทางไปยังหน้าหลักของแต่ละ Role (รองรับแยก Username ของแต่ละแท็บอิสระ)
        private IActionResult RedirectToRoleHome(string? role, string? username = null)
        {
            var canonical = DemoUserHelper.GetDemoUserInfo(role);
            string tabRoleKey = (!string.IsNullOrWhiteSpace(username) &&
                                 !string.Equals(username, canonical.Username, System.StringComparison.OrdinalIgnoreCase))
                ? username
                : (role ?? "Customer");

            return role switch
            {
                nameof(UserRole.InventoryStaff) => RedirectToAction("Index", "Inventory", new { role = tabRoleKey }),
                nameof(UserRole.OrderStaff) => RedirectToAction("Index", "Order", new { role = tabRoleKey }),
                nameof(UserRole.ShippingStaff) => RedirectToAction("Index", "Shipping", new { role = tabRoleKey }),
                nameof(UserRole.MarketingStaff) => RedirectToAction("Index", "Marketing", new { role = tabRoleKey }),
                nameof(UserRole.AccountingStaff) => RedirectToAction("Index", "Accounting", new { role = tabRoleKey }),
                nameof(UserRole.ITSupport) => RedirectToAction("Index", "Support", new { role = tabRoleKey }),
                nameof(UserRole.Admin) => RedirectToAction("Index", "Admin", new { role = tabRoleKey }),
                nameof(UserRole.Owner) => RedirectToAction("Index", "Owner", new { role = tabRoleKey }),
                _ => RedirectToAction("Index", "Home", new { role = tabRoleKey })
            };
        }
    }
}
