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
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.TotalPromotions = _repo.Promotions.Count;
            ViewBag.ActivePromotionsCount = _repo.Promotions.Count(p => p.IsValidNow);
            ViewBag.TotalUsageAllPromos = _repo.Promotions.Sum(p => p.UsageCount);

            return View(_repo.Promotions);
        }

        // สวิตช์เปิด/ปิดโปรโมชั่น (Req 5)
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var promo = _repo.Promotions.FirstOrDefault(p => p.Id == id);
            if (promo != null)
            {
                promo.IsActive = !promo.IsActive;
                _repo.SaveDatabase();
                TempData["SuccessMessage"] = $"ปรับสถานะโปรโมชั่น '{promo.Title}' เป็น {(promo.IsActive ? "เปิดใช้งาน (Active)" : "ปิดชั่วคราว (Inactive)")} เรียบร้อยแล้ว";
            }
            return RedirectToAction(nameof(Index));
        }

        // สร้างโปรโมชั่นใหม่ (Req 25, 26)
        [HttpPost]
        public IActionResult Create(Promotion model)
        {
            model.Id = _repo.Promotions.Any() ? _repo.Promotions.Max(p => p.Id) + 1 : 1;
            model.UsageCount = 0;
            _repo.Promotions.Add(model);
            _repo.SaveDatabase();

            TempData["SuccessMessage"] = $"สร้างแคมเปญโปรโมชั่น '{model.Title}' สำเร็จแล้ว";
            return RedirectToAction(nameof(Index));
        }
    }
}
