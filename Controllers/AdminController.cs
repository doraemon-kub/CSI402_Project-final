using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using CSI402_Project_final.Models;
using CSI402_Project_final.Hubs;
using System.Linq;
using System.Threading.Tasks;

namespace CSI402_Project_final.Controllers
{
    [Authorize(Roles = "Admin,Owner")]
    public class AdminController : Controller
    {
        private readonly PetShopRepository _repo;
        private readonly IHubContext<PetShopHub> _hubContext;

        public AdminController(PetShopRepository repo, IHubContext<PetShopHub> hubContext)
        {
            _repo = repo;
            _hubContext = hubContext;
        }

        // ตรวจสอบว่าผู้ใช้งานปัจจุบันที่กำลังทำรายการมีสถานะเป็น Owner หรือไม่ (รองรับทั้ง Claims และ Multi-Role Tab Switcher)
        private bool IsCurrentOperatorOwner(string? roleParam)
        {
            if (!string.IsNullOrEmpty(roleParam))
            {
                var resolved = _repo.ResolveTabUserInfo(roleParam);
                return string.Equals(resolved.RoleName, "Owner", System.StringComparison.OrdinalIgnoreCase);
            }
            return User.IsInRole("Owner");
        }

        // หน้าจัดการผู้ใช้และสิทธิ์การเข้าถึง (Admin) พร้อมระบบกรองตามบทบาท (Role Filter) & ค้นหา
        public IActionResult Index(string? role = null, string? roleFilter = "all", string? search = null)
        {
            var allUsers = _repo.Users;
            foreach (var u in allUsers)
            {
                _repo.CheckAndRefreshUserSuspension(u);
            }
            allUsers = _repo.Users;

            bool isOwner = IsCurrentOperatorOwner(role);
            ViewBag.IsCurrentOwner = isOwner;
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.TotalUsers = allUsers.Count;
            ViewBag.TotalStaff = allUsers.Count(u => u.Role != UserRole.Customer && u.Role != UserRole.Guest);
            ViewBag.TotalCustomers = allUsers.Count(u => u.Role == UserRole.Customer);

            string filter = (roleFilter ?? "all").Trim().ToLowerInvariant();
            IEnumerable<SystemUser> filtered = allUsers;

            if (filter == "staff")
            {
                filtered = filtered.Where(u => u.Role != UserRole.Customer && u.Role != UserRole.Guest);
            }
            else if (filter == "customer")
            {
                filtered = filtered.Where(u => u.Role == UserRole.Customer);
            }
            else if (Enum.TryParse<UserRole>(filter, true, out var specificRole))
            {
                filtered = filtered.Where(u => u.Role == specificRole);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim();
                filtered = filtered.Where(u =>
                    u.Username.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
                    u.FullName.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(kw, System.StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.RoleFilter = filter;
            ViewBag.Search = search;

            return View(filtered.ToList());
        }

        // ปรับเปลี่ยนบทบาทและสิทธิ์ของพนักงาน + ระงับบัญชีพร้อมเหตุผลและระยะเวลา (เตะออกทันทีผ่าน SignalR)
        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(
            int userId,
            UserRole newRole,
            bool isActive,
            string? suspendReason = null,
            int suspendDurationMinutes = -1,
            string? role = null,
            string? roleFilter = null,
            string? search = null)
        {
            bool isOwner = IsCurrentOperatorOwner(role);
            var targetUser = _repo.Users.FirstOrDefault(u => u.Id == userId);
            if (targetUser == null)
            {
                TempData["ErrorMessage"] = "ไม่พบข้อมูลผู้ใช้งานที่ต้องการแก้ไข";
                return RedirectToAction(nameof(Index), new { role, roleFilter, search });
            }

            // กฎความปลอดภัย: Admin ไม่สามารถแก้ไข/ระงับบัญชี Admin หรือ Owner ได้ และไม่สามารถแต่งตั้งใครเป็น Admin หรือ Owner ได้ (เฉพาะ Owner เท่านั้น)
            if (!isOwner)
            {
                if (targetUser.Role == UserRole.Admin || targetUser.Role == UserRole.Owner)
                {
                    TempData["ErrorMessage"] = "ไม่สามารถแก้ไขหรือระงับบัญชี Admin หรือ Owner ได้: สิทธิ์นี้สงวนไว้สำหรับผู้บริหารสูงสุด (Owner) เท่านั้น";
                    return RedirectToAction(nameof(Index), new { role, roleFilter, search });
                }
                if (newRole == UserRole.Admin || newRole == UserRole.Owner)
                {
                    TempData["ErrorMessage"] = "ไม่สามารถแต่งตั้งผู้ใช้เป็น Admin หรือ Owner ได้: สิทธิ์นี้สงวนไว้สำหรับผู้บริหารสูงสุด (Owner) เท่านั้น";
                    return RedirectToAction(nameof(Index), new { role, roleFilter, search });
                }
            }

            if (_repo.UpdateUserRole(userId, newRole, isActive, suspendReason, suspendDurationMinutes))
            {
                if (!isActive)
                {
                    string reasonText = targetUser.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ";
                    string untilText = targetUser.SuspensionDisplayUntil;

                    // ส่งสัญญาณ Real-Time เตะผู้ใช้ออกจากระบบทุกแท็บที่กำลังล็อกอินบัญชีนี้อยู่ทันที
                    await _hubContext.Clients.All.SendAsync(
                        "ReceiveAccountSuspended",
                        targetUser.Username,
                        targetUser.Role.ToString(),
                        reasonText,
                        untilText);

                    TempData["SuccessMessage"] = $"ระงับบัญชี '{targetUser.Username}' เรียบร้อยแล้ว ({untilText} | เหตุผล: {reasonText})";
                }
                else
                {
                    TempData["SuccessMessage"] = $"อัปเดตบัญชีผู้ใช้งาน #{userId} ({targetUser.Username}) เป็น '{newRole}' (สถานะ: เปิดใช้งานปกติ) สำเร็จ";
                }
            }
            return RedirectToAction(nameof(Index), new { role, roleFilter, search });
        }

        // เพิ่มผู้ใช้งานใหม่ (บันทึกลง DB) - Admin ไม่สามารถสร้าง User บทบาท Admin ได้ ต้องให้ Owner ทำเท่านั้น
        [HttpPost]
        public IActionResult CreateUser(SystemUser user, string? newUserRole = null, string? role = null)
        {
            // ใช้ชื่อฟิลด์ newUserRole แยกจาก role (บทบาทของแท็บผู้ทำรายการ) เพื่อไม่ให้ Model Binding ชนกัน
            if (string.IsNullOrWhiteSpace(newUserRole) || !Enum.TryParse<UserRole>(newUserRole, true, out var requestedRole))
            {
                TempData["ErrorMessage"] = "กรุณาเลือกบทบาทหน้าที่ (Role) ให้ถูกต้อง";
                return RedirectToAction(nameof(Index), new { role });
            }
            user.Role = requestedRole;

            bool isOwner = IsCurrentOperatorOwner(role);
            if ((requestedRole == UserRole.Admin || requestedRole == UserRole.Owner) && !isOwner)
            {
                TempData["ErrorMessage"] = "ไม่สามารถสร้างบัญชีบทบาท Admin ได้: สิทธิ์นี้สงวนไว้สำหรับผู้บริหารสูงสุด (Owner) เท่านั้น";
                return RedirectToAction(nameof(Index), new { role });
            }

            var (success, message) = _repo.CreateUser(user);
            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction(nameof(Index), new { role });
        }

        // หน้าตรวจสอบบันทึกกิจกรรมย้อนหลัง (Audit Log)
        public IActionResult AuditLogs(string? search = null, string? roleFilter = null, string? actionFilter = null, string? role = null)
        {
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            var logs = _repo.GetAuditLogs(500);

            ViewBag.TotalLogsCount = logs.Count;
            ViewBag.SecurityLogsCount = logs.Count(l => l.Action == "UPDATE_USER_ROLE" || l.Action == "SUSPEND_ACCOUNT" || l.Action == "CREATE_USER");
            ViewBag.FinanceLogsCount = logs.Count(l => l.Action.Contains("PAYMENT") || l.Action.Contains("REFUND") || l.Action.Contains("EXPENSE"));
            ViewBag.InventoryLogsCount = logs.Count(l => l.Action.Contains("PRODUCT") || l.Action.Contains("STOCK"));

            if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "all")
            {
                logs = logs.Where(l => string.Equals(l.UserRole, roleFilter, System.StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(actionFilter) && actionFilter != "all")
            {
                logs = logs.Where(l => string.Equals(l.Action, actionFilter, System.StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim();
                logs = logs.Where(l =>
                    (!string.IsNullOrEmpty(l.Username) && l.Username.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.Action) && l.Action.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.TableName) && l.TableName.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.RecordId) && l.RecordId.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.Details) && l.Details.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.IpAddress) && l.IpAddress.Contains(kw, System.StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            ViewBag.Search = search;
            ViewBag.RoleFilter = roleFilter ?? "all";
            ViewBag.ActionFilter = actionFilter ?? "all";
            ViewBag.FilteredCount = logs.Count;

            return View(logs);
        }
    }
}
