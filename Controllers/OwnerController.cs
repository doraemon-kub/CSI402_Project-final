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
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;

            // คำนวณสรุปทางการเงิน
            var financial = _repo.GetFinancialSummary();
            ViewBag.Financial = financial;

            // สถิติยอดขายตามประเภทสัตว์เลี้ยง
            int dogProductOrders = _repo.Orders.SelectMany(o => o.Items)
                .Count(i => _repo.Products.Any(p => p.Id == i.ProductId && p.PetType == PetCategory.Dog));
            int catProductOrders = _repo.Orders.SelectMany(o => o.Items)
                .Count(i => _repo.Products.Any(p => p.Id == i.ProductId && p.PetType == PetCategory.Cat));
            ViewBag.DogOrderCount = dogProductOrders;
            ViewBag.CatOrderCount = catProductOrders;

            // สินค้าขายดี Top 5 (คำนวณจากจำนวนคนใส่ตะกร้า + ออเดอร์)
            var topProducts = _repo.Products
                .OrderByDescending(p => p.CartAddCount)
                .Take(5)
                .ToList();
            ViewBag.TopProducts = topProducts;

            // แคมเปญโปรโมชั่นยอดนิยม
            var topPromotions = _repo.Promotions
                .OrderByDescending(p => p.UsageCount)
                .Take(4)
                .ToList();
            ViewBag.TopPromotions = topPromotions;

            // จำนวนออเดอร์ทั้งหมด
            ViewBag.TotalOrderCount = _repo.Orders.Count;
            ViewBag.RecentOrders = _repo.Orders.OrderByDescending(o => o.OrderDate).Take(5).ToList();

            return View();
        }
    }
}
