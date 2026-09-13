using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Models;
using System.Linq;

namespace CSI402_Project_final.Controllers
{
    [Authorize(Roles = "InventoryStaff,Owner,Admin")]
    public class InventoryController : Controller
    {
        private readonly PetShopRepository _repo;

        public InventoryController(PetShopRepository repo)
        {
            _repo = repo;
        }

        // หน้าหลักคลังสินค้า: สต็อกสินค้า & แจ้งเตือนสินค้าต่ำกว่าระดับที่กำหนด (Req 18)
        public IActionResult Index()
        {
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.LowStockCount = _repo.Products.Count(p => p.IsLowStock);
            ViewBag.OutOfStockCount = _repo.Products.Count(p => p.IsOutOfStock);
            ViewBag.TotalProducts = _repo.Products.Count;

            var products = _repo.Products.OrderBy(p => p.StockQuantity).ToList();
            return View(products);
        }

        // อัปเดตสต็อกรวดเร็ว
        [HttpPost]
        public IActionResult UpdateStock(int id, int adjustment)
        {
            var product = _repo.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                product.StockQuantity = System.Math.Max(0, product.StockQuantity + adjustment);
                _repo.SaveDatabase();
                TempData["SuccessMessage"] = $"อัปเดตสต็อกสินค้า '{product.Name}' เป็น {product.StockQuantity} ชิ้น เรียบร้อยแล้ว";
            }
            return RedirectToAction(nameof(Index));
        }

        // เพิ่มหรือแก้ไขสินค้าใหม่ (Req 6, 17)
        [HttpPost]
        public IActionResult SaveProduct(Product model)
        {
            if (model.Id == 0)
            {
                model.Id = _repo.Products.Any() ? _repo.Products.Max(p => p.Id) + 1 : 1;
                if (string.IsNullOrWhiteSpace(model.ImageUrl))
                {
                    model.ImageUrl = "https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=500&q=80";
                }
                _repo.Products.Add(model);
                _repo.SaveDatabase();
                TempData["SuccessMessage"] = $"เพิ่มสินค้า '{model.Name}' สำเร็จ";
            }
            else
            {
                var existing = _repo.Products.FirstOrDefault(p => p.Id == model.Id);
                if (existing != null)
                {
                    existing.Name = model.Name;
                    existing.Brand = model.Brand;
                    existing.PetType = model.PetType;
                    existing.Category = model.Category;
                    existing.WeightKg = model.WeightKg;
                    existing.Price = model.Price;
                    existing.SpecialDiscountPrice = model.SpecialDiscountPrice;
                    existing.AgeStage = model.AgeStage;
                    existing.SpecialNeed = model.SpecialNeed;
                    existing.StockQuantity = model.StockQuantity;
                    existing.LowStockThreshold = model.LowStockThreshold;
                    existing.Description = model.Description;
                    _repo.SaveDatabase();
                    TempData["SuccessMessage"] = $"แก้ไขข้อมูลสินค้า '{model.Name}' สำเร็จ";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            var existing = _repo.Products.FirstOrDefault(p => p.Id == id);
            if (existing != null)
            {
                _repo.Products.Remove(existing);
                _repo.SaveDatabase();
                TempData["SuccessMessage"] = $"ลบสินค้า '{existing.Name}' เรียบร้อยแล้ว";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
