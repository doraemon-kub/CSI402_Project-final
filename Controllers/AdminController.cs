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

        // หน้าจัดการผู้ใช้และสิทธิ์การเข้าถึง (Admin) พร้อมระบบกรองตามบทบาท (Role Filter) & ค้นหา
        public IActionResult Index(string? roleFilter = "all", string? search = null)
        {
            var allUsers = _repo.Users;
            foreach (var u in allUsers)
            {
                _repo.CheckAndRefreshUserSuspension(u);
            }
            allUsers = _repo.Users;

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
            if (_repo.UpdateUserRole(userId, newRole, isActive, suspendReason, suspendDurationMinutes))
            {
                var targetUser = _repo.Users.FirstOrDefault(u => u.Id == userId);
                if (targetUser != null && !isActive)
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
                    TempData["SuccessMessage"] = $"อัปเดตบัญชีผู้ใช้งาน #{userId} ({targetUser?.Username}) เป็น '{newRole}' (สถานะ: เปิดใช้งานปกติ) สำเร็จ";
                }
            }
            return RedirectToAction(nameof(Index), new { role, roleFilter, search });
        }

        // เพิ่มผู้ใช้งานใหม่ (บันทึกลง DB)
        [HttpPost]
        public IActionResult CreateUser(SystemUser user, string? role = null)
        {
            var (success, message) = _repo.CreateUser(user);
            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction(nameof(Index), new { role });
        }
    }
}
