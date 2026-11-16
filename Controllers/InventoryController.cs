using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Helpers;
using CSI402_Project_final.Models;
using System.Linq;
using System.Threading.Tasks;

namespace CSI402_Project_final.Controllers
{
    [Authorize(Roles = "InventoryStaff,Owner,Admin")]
    public class InventoryController : Controller
    {
        private readonly PetShopRepository _repo;
        private readonly IWebHostEnvironment _env;

        public InventoryController(PetShopRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        // หน้าหลักคลังสินค้า: สต็อกสินค้า & แจ้งเตือนสินค้าต่ำกว่าระดับที่กำหนด (Req 18)
        public IActionResult Index()
        {
            var products = _repo.Products.OrderBy(p => p.StockQuantity).ToList();

            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.LowStockCount = products.Count(p => p.IsLowStock);
            ViewBag.OutOfStockCount = products.Count(p => p.IsOutOfStock);
            ViewBag.TotalProducts = products.Count;

            return View(products);
        }

        // อัปเดตสต็อกรวดเร็ว
        [HttpPost]
        public IActionResult UpdateStock(int id, int adjustment)
        {
            var product = _repo.UpdateProductStock(id, adjustment);
            if (product != null)
            {
                TempData["SuccessMessage"] = $"อัปเดตสต็อกสินค้า '{product.Name}' เป็น {product.StockQuantity} ชิ้น เรียบร้อยแล้ว";
            }
            return RedirectToAction(nameof(Index));
        }

        // เพิ่มหรือแก้ไขสินค้า (Req 6, 17) รองรับอัปโหลดรูปภาพและลิงก์รูปภาพ
        [HttpPost]
        public async Task<IActionResult> SaveProduct(Product model, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(imageFile, _env.WebRootPath, "products");
                if (uploadErr != null)
                {
                    TempData["ErrorMessage"] = uploadErr;
                    return RedirectToAction(nameof(Index));
                }
                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    model.ImageUrl = uploadedUrl;
                }
            }

            if (_repo.SaveProduct(model, out var message))
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            var (success, message) = _repo.DeleteProduct(id);
            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction(nameof(Index));
        }
    }
}
