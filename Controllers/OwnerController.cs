using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Models;
using System.Linq;

namespace CSI402_Project_final.Controllers
{
    [Authorize(Roles = "Owner")]
    public class OwnerController : Controller
    {
        private readonly PetShopRepository _repo;

        public OwnerController(PetShopRepository repo)
        {
            _repo = repo;
        }

        // หน้า Executive Dashboard ภาพรวมธุรกิจ (Owner)
        public IActionResult Index()
        {
            var orders = _repo.Orders;
            var products = _repo.Products;
            var promotions = _repo.Promotions;

            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.Financial = _repo.GetFinancialSummary();

            var productTypesById = products.ToDictionary(p => p.Id, p => p.PetType);
            int dogProductOrders = orders.SelectMany(o => o.Items)
                .Count(i => productTypesById.TryGetValue(i.ProductId, out var pt) && pt == PetCategory.Dog);
            int catProductOrders = orders.SelectMany(o => o.Items)
                .Count(i => productTypesById.TryGetValue(i.ProductId, out var pt) && pt == PetCategory.Cat);
            int otherProductOrders = orders.SelectMany(o => o.Items)
                .Count(i => productTypesById.TryGetValue(i.ProductId, out var pt) && pt == PetCategory.Other);

            ViewBag.DogOrderCount = dogProductOrders;
            ViewBag.CatOrderCount = catProductOrders;
            ViewBag.OtherOrderCount = otherProductOrders;

            // สัดส่วนสินค้าในคลังและยอดสั่งซื้อแยกตามหมวดหมู่ (อาหาร / ของใช้สัตว์เลี้ยง / สุขอนามัย)
            ViewBag.FoodCatalogCount = products.Count(p => p.Category == ProductCategory.Food);
            ViewBag.SupplyCatalogCount = products.Count(p => p.Category == ProductCategory.Supply);
            ViewBag.HygieneCatalogCount = products.Count(p => p.Category == ProductCategory.Hygiene);

            var productCatById = products.ToDictionary(p => p.Id, p => p.Category);
            ViewBag.FoodOrderCount = orders.SelectMany(o => o.Items)
                .Count(i => productCatById.TryGetValue(i.ProductId, out var cat) && cat == ProductCategory.Food);
            ViewBag.SupplyOrderCount = orders.SelectMany(o => o.Items)
                .Count(i => productCatById.TryGetValue(i.ProductId, out var cat) && cat == ProductCategory.Supply);
            ViewBag.HygieneOrderCount = orders.SelectMany(o => o.Items)
                .Count(i => productCatById.TryGetValue(i.ProductId, out var cat) && cat == ProductCategory.Hygiene);

            var topProducts = products
                .OrderByDescending(p => p.CartAddCount)
                .Take(5)
                .ToList();
            ViewBag.TopProducts = topProducts;

            var topPromotions = promotions
                .OrderByDescending(p => p.UsageCount)
                .Take(4)
                .ToList();
            ViewBag.TopPromotions = topPromotions;

            ViewBag.TotalOrderCount = orders.Count;
            ViewBag.RecentOrders = orders.Take(5).ToList();

            return View();
        }
    }
}
