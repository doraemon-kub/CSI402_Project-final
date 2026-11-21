using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Models;
using System.Linq;

namespace CSI402_Project_final.Controllers
{
    [Authorize(Roles = "Admin,Owner")]
    public class AdminController : Controller
    {
        private readonly PetShopRepository _repo;

        public AdminController(PetShopRepository repo)
        {
            _repo = repo;
        }

        // หน้าจัดการผู้ใช้และสิทธิ์การเข้าถึง (Admin)
        public IActionResult Index()
        {
            var users = _repo.Users;
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.TotalStaff = users.Count(u => u.Role != UserRole.Customer && u.Role != UserRole.Guest);
            ViewBag.TotalCustomers = users.Count(u => u.Role == UserRole.Customer);

            return View(users);
        }

        // ปรับเปลี่ยนบทบาทและสิทธิ์ของพนักงาน (บันทึกลง DB)
        [HttpPost]
        public IActionResult UpdateUserRole(int userId, UserRole newRole, bool isActive, string? role = null)
        {
            if (_repo.UpdateUserRole(userId, newRole, isActive))
            {
                TempData["SuccessMessage"] = $"อัปเดตบทบาทผู้ใช้งาน #{userId} เป็น '{newRole}' สำเร็จ";
            }
            return RedirectToAction(nameof(Index), new { role });
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
