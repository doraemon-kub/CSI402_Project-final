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

        // หน้าร้านหลัก พร้อมฟิลเตอร์แยกประเภทสัตว์ (Req 1), หมวดหมู่ของใช้/อาหาร, น้ำหนัก (Req 2), ช่วงวัย/โรค (Req 7, 9, 10)
        public IActionResult Index(
            PetCategory? petType = null,
            ProductCategory? category = null,
            AgeStage? ageStage = null,
            string? specialNeed = null,
            double? maxWeight = null,
            string? search = null)
        {
            var allCatalog = _repo.QueryProducts().ToList();

            // คำนวณจำนวนสินค้าแยกตามหมวดหมู่ (ตามประเภทสัตว์ที่เลือกอยู่)
            var baseForCategoryCount = petType.HasValue
                ? allCatalog.Where(p => p.PetType == petType.Value).ToList()
                : allCatalog;

            ViewBag.AllCategoryCount = baseForCategoryCount.Count;
            ViewBag.FoodCount = baseForCategoryCount.Count(p => p.Category == ProductCategory.Food);
            ViewBag.SupplyCount = baseForCategoryCount.Count(p => p.Category == ProductCategory.Supply);
            ViewBag.HygieneCount = baseForCategoryCount.Count(p => p.Category == ProductCategory.Hygiene);

            IEnumerable<Product> products = allCatalog;

            if (petType.HasValue)
                products = products.Where(p => p.PetType == petType.Value);

            if (category.HasValue)
                products = products.Where(p => p.Category == category.Value);

            if (ageStage.HasValue && ageStage.Value != AgeStage.AllAges)
                products = products.Where(p => p.AgeStage == ageStage.Value || p.AgeStage == AgeStage.AllAges);

            if (!string.IsNullOrWhiteSpace(specialNeed))
                products = products.Where(p => p.SpecialNeed.Contains(specialNeed, System.StringComparison.OrdinalIgnoreCase));

            if (maxWeight.HasValue)
                products = products.Where(p => p.WeightKg <= maxWeight.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                products = products.Where(p =>
                    p.Name.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||
                    p.Brand.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||
                    p.SpecialNeed.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(term, System.StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.ActivePromotions = _repo.GetValidPromotions();
            ViewBag.CurrentCartCount = _repo.CurrentCart.TotalItemCount;
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.SelectedPetType = petType;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedAgeStage = ageStage;
            ViewBag.SelectedSpecialNeed = specialNeed;
            ViewBag.SelectedMaxWeight = maxWeight;
            ViewBag.SelectedSearch = search;

            return View(products.ToList());
        }

        // หน้ารายละเอียดสินค้า (แสดง Layer ช่วงวัย & Social proof คนใส่ตะกร้า Req 7, 8, 10)
        public IActionResult Details(int id)
        {
            var allProducts = _repo.QueryProducts().ToList();
            var product = allProducts.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            // สินค้าแนะนำในหมวดหมู่และช่วงวัยเดียวกัน (รองรับทั้งอาหารและของใช้สัตว์เลี้ยง)
            var related = allProducts
                .Where(p => p.Id != id && p.PetType == product.PetType && p.Category == product.Category)
                .OrderByDescending(p => p.AgeStage == product.AgeStage)
                .Take(4)
                .ToList();

            if (related.Count < 4)
            {
                var additional = allProducts
                    .Where(p => p.Id != id && !related.Any(r => r.Id == p.Id) && (p.Category == product.Category || p.PetType == product.PetType))
                    .Take(4 - related.Count);
                related.AddRange(additional);
            }

            ViewBag.RelatedProducts = related;
            ViewBag.CurrentCartCount = _repo.CurrentCart.TotalItemCount;
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;

            return View(product);
        }

        [HttpPost]
        public IActionResult SwitchRole(UserRole role, string returnUrl = "/")
        {
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
        }
    }
}
