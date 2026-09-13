using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Models;
using System.Linq;

namespace CSI402_Project_final.Controllers
{
    public class HomeController : Controller
    {
        private readonly PetShopRepository _repo;

        public HomeController(PetShopRepository repo)
        {
            _repo = repo;
        }

        // หน้าร้านหลัก พร้อมฟิลเตอร์แยกประเภทสัตว์ (Req 1), น้ำหนัก (Req 2), ช่วงวัย/โรค (Req 7, 9, 10)
        public IActionResult Index(
            PetCategory? petType = null, 
            ProductCategory? category = null, 
            AgeStage? ageStage = null,
            string? specialNeed = null,
            double? maxWeight = null,
            string? search = null)
        {
            var products = _repo.Products.AsQueryable();

            if (petType.HasValue)
                products = products.Where(p => p.PetType == petType.Value);

            if (category.HasValue)
                products = products.Where(p => p.Category == category.Value);

            if (ageStage.HasValue && ageStage.Value != AgeStage.AllAges)
                products = products.Where(p => p.AgeStage == ageStage.Value || p.AgeStage == AgeStage.AllAges);

            if (!string.IsNullOrWhiteSpace(specialNeed))
                products = products.Where(p => p.SpecialNeed.Contains(specialNeed));

            if (maxWeight.HasValue)
                products = products.Where(p => p.WeightKg <= maxWeight.Value);

            if (!string.IsNullOrWhiteSpace(search))
                products = products.Where(p => p.Name.Contains(search, System.StringComparison.OrdinalIgnoreCase) || 
                                               p.Brand.Contains(search, System.StringComparison.OrdinalIgnoreCase));

            ViewBag.ActivePromotions = _repo.Promotions.Where(p => p.IsValidNow).ToList();
            ViewBag.CurrentCartCount = _repo.CurrentCart.TotalItemCount;
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.SelectedPetType = petType;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedAgeStage = ageStage;
            ViewBag.SelectedSpecialNeed = specialNeed;

            return View(products.ToList());
        }

        // หน้ารายละเอียดสินค้า (แสดง Layer ช่วงวัย & Social proof คนใส่ตะกร้า Req 7, 8, 10)
        public IActionResult Details(int id)
        {
            var product = _repo.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            // สินค้าแนะนำในช่วงวัยเดียวกัน (Req 7, 10)
            ViewBag.RelatedProducts = _repo.Products
                .Where(p => p.Id != id && p.PetType == product.PetType && p.AgeStage == product.AgeStage)
                .Take(4)
                .ToList();

            ViewBag.CurrentCartCount = _repo.CurrentCart.TotalItemCount;
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;

            return View(product);
        }

        // ระบบสลับมุมมอง Role เพื่อให้ทดสอบทั้ง 10 บทบาทได้ทันที
        [HttpPost]
        public IActionResult SwitchRole(UserRole role, string returnUrl = "/")
        {
            _repo.CurrentSimulatedRole = role;
            return LocalRedirect(returnUrl);
        }
    }
}
