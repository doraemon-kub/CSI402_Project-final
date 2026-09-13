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
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.TotalStaff = _repo.Users.Count(u => u.Role != UserRole.Customer && u.Role != UserRole.Guest);
            ViewBag.TotalCustomers = _repo.Users.Count(u => u.Role == UserRole.Customer);

            return View(_repo.Users);
        }

        // ปรับเปลี่ยนบทบาทและสิทธิ์ของพนักงาน
        [HttpPost]
        public IActionResult UpdateUserRole(int userId, UserRole newRole, bool isActive)
        {
            var user = _repo.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.Role = newRole;
                user.IsActive = isActive;
                TempData["SuccessMessage"] = $"อัปเดตบทบาทของ '{user.FullName}' เป็น '{newRole}' สำเร็จ";
            }
            return RedirectToAction(nameof(Index));
        }

        // เพิ่มผู้ใช้งานใหม่
        [HttpPost]
        public IActionResult CreateUser(SystemUser user)
        {
            user.Id = _repo.Users.Any() ? _repo.Users.Max(u => u.Id) + 1 : 1;
            _repo.Users.Add(user);
            TempData["SuccessMessage"] = $"เพิ่มผู้ใช้งาน '{user.FullName}' ในระบบเรียบร้อยแล้ว";
            return RedirectToAction(nameof(Index));
        }
    }
}
