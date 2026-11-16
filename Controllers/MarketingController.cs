using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Models;
using System.Linq;

namespace CSI402_Project_final.Controllers
{
    [Authorize(Roles = "MarketingStaff,Owner,Admin")]
    public class MarketingController : Controller
    {
        private readonly PetShopRepository _repo;

        public MarketingController(PetShopRepository repo)
        {
            _repo = repo;
        }

        // หน้าแดชบอร์ดโปรโมชั่น & สถิติ (Req 5, 27)
        public IActionResult Index()
        {
            var promotions = _repo.Promotions;
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.TotalPromotions = promotions.Count;
            ViewBag.ActivePromotionsCount = promotions.Count(p => p.IsValidNow);
            ViewBag.TotalUsageAllPromos = promotions.Sum(p => p.UsageCount);

            return View(promotions);
        }

        // สวิตช์เปิด/ปิดโปรโมชั่น (Req 5)
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var promo = _repo.TogglePromotionStatus(id);
            if (promo != null)
            {
                TempData["SuccessMessage"] = $"ปรับสถานะโปรโมชั่น '{promo.Title}' เป็น {(promo.IsActive ? "เปิดใช้งาน (Active)" : "ปิดชั่วคราว (Inactive)")} เรียบร้อยแล้ว";
            }
            return RedirectToAction(nameof(Index));
        }

        // สร้างหรือแก้ไขแคมเปญโปรโมชั่น (Req 5, 25, 26)
        [HttpPost]
        public IActionResult Save(Promotion model)
        {
            var (success, message) = _repo.SavePromotion(model);
            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Create(Promotion model) => Save(model);

        // ลบโปรโมชั่น (Req 5)
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var (success, message) = _repo.DeletePromotion(id);
            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction(nameof(Index));
        }
    }
}
