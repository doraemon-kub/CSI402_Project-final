using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using CSI402_Project_final.Helpers;
using CSI402_Project_final.Hubs;
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
        private readonly IHubContext<PetShopHub> _hubContext;

        public InventoryController(PetShopRepository repo, IWebHostEnvironment env, IHubContext<PetShopHub> hubContext)
        {
            _repo = repo;
            _env = env;
            _hubContext = hubContext;
        }

        // หน้าหลักคลังสินค้า: สต็อกสินค้า & แจ้งเตือนสินค้าต่ำกว่าระดับที่กำหนด (Req 18) พร้อมตัวกรอง Low Stock / Out of Stock
        public IActionResult Index(string? stockFilter = "all", string? search = null)
        {
            var allProducts = _repo.Products.OrderBy(p => p.StockQuantity).ToList();

            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.TotalProducts = allProducts.Count;
            ViewBag.LowStockCount = allProducts.Count(p => p.IsLowStock);
            ViewBag.OutOfStockCount = allProducts.Count(p => p.IsOutOfStock);
            ViewBag.NormalStockCount = allProducts.Count(p => !p.IsLowStock && !p.IsOutOfStock);

            string normalizedFilter = (stockFilter ?? "all").Trim().ToLowerInvariant();
            IEnumerable<Product> filtered = allProducts;

            filtered = normalizedFilter switch
            {
                "low" => filtered.Where(p => p.IsLowStock),
                "out" => filtered.Where(p => p.IsOutOfStock),
                "normal" => filtered.Where(p => !p.IsLowStock && !p.IsOutOfStock),
                "alert" => filtered.Where(p => p.IsLowStock || p.IsOutOfStock),
                _ => filtered
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim();
                filtered = filtered.Where(p =>
                    p.Name.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
                    p.Brand.Contains(kw, System.StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.StockFilter = normalizedFilter;
            ViewBag.Search = search;

            return View(filtered.ToList());
        }

        // อัปเดตสต็อกรวดเร็ว (คงสถานะ Filter เดิมไว้)
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int id, int adjustment, string? stockFilter = null, string? search = null, string? role = null)
        {
            var product = _repo.UpdateProductStock(id, adjustment);
            if (product != null)
            {
                TempData["SuccessMessage"] = $"อัปเดตสต็อกสินค้า '{product.Name}' เป็น {product.StockQuantity} ชิ้น เรียบร้อยแล้ว";

                // SignalR Real-Time Socket Broadcast: แจ้งเตือนไปยังทุกหน้าจอและลูกค้า
                await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", product.Id, product.StockQuantity);
                if (product.IsLowStock)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", product.Name, product.StockQuantity);
                }
            }
            return RedirectToAction(nameof(Index), new { stockFilter, search, role });
        }

        // เพิ่มหรือแก้ไขสินค้า (Req 6, 17) รองรับอัปโหลดรูปภาพและลิงก์รูปภาพ
        [HttpPost]
        public async Task<IActionResult> SaveProduct(Product model, IFormFile? imageFile, string? role = null)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(imageFile, _env.WebRootPath, "products");
                if (uploadErr != null)
                {
                    TempData["ErrorMessage"] = uploadErr;
                    return RedirectToAction(nameof(Index), new { role });
                }
                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    model.ImageUrl = uploadedUrl;
                }
            }

            if (_repo.SaveProduct(model, out var message))
            {
                TempData["SuccessMessage"] = message;

                // SignalR Real-Time Socket Broadcast
                await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", model.Id, model.StockQuantity);
                if (model.IsLowStock)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", model.Name, model.StockQuantity);
                }
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }
            return RedirectToAction(nameof(Index), new { role });
        }

        [HttpPost]
        public IActionResult DeleteProduct(int id, string? role = null)
        {
            var (success, message) = _repo.DeleteProduct(id);
            if (success)
                TempData["SuccessMessage"] = message;
            else
                TempData["ErrorMessage"] = message;

            return RedirectToAction(nameof(Index), new { role });
        }
    }
}
