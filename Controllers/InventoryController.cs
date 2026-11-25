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

        // หน้าหลักคลังสินค้า: สต็อกสินค้า & แจ้งเตือนสินค้าต่ำกว่าระดับที่กำหนด (Req 18) พร้อมตัวกรอง Low Stock / Out of Stock และกรองหมวดหมู่ของใช้/อาหาร
        public IActionResult Index(string? stockFilter = "all", string? categoryFilter = "all", string? search = null)
        {
            var allProducts = _repo.Products.OrderBy(p => p.StockQuantity).ToList();

            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.TotalProducts = allProducts.Count;
            ViewBag.LowStockCount = allProducts.Count(p => p.IsLowStock);
            ViewBag.OutOfStockCount = allProducts.Count(p => p.IsOutOfStock);
            ViewBag.NormalStockCount = allProducts.Count(p => !p.IsLowStock && !p.IsOutOfStock);

            ViewBag.FoodCount = allProducts.Count(p => p.Category == ProductCategory.Food);
            ViewBag.SupplyCount = allProducts.Count(p => p.Category == ProductCategory.Supply);
            ViewBag.HygieneCount = allProducts.Count(p => p.Category == ProductCategory.Hygiene);

            string normalizedFilter = (stockFilter ?? "all").Trim().ToLowerInvariant();
            string normalizedCategory = (categoryFilter ?? "all").Trim();
            IEnumerable<Product> filtered = allProducts;

            filtered = normalizedFilter switch
            {
                "low" => filtered.Where(p => p.IsLowStock),
                "out" => filtered.Where(p => p.IsOutOfStock),
                "normal" => filtered.Where(p => !p.IsLowStock && !p.IsOutOfStock),
                "alert" => filtered.Where(p => p.IsLowStock || p.IsOutOfStock),
                _ => filtered
            };

            if (!string.Equals(normalizedCategory, "all", System.StringComparison.OrdinalIgnoreCase) &&
                System.Enum.TryParse<ProductCategory>(normalizedCategory, true, out var parsedCat))
            {
                filtered = filtered.Where(p => p.Category == parsedCat);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim();
                filtered = filtered.Where(p =>
                    p.Name.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
                    p.Brand.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
                    p.SpecialNeed.Contains(kw, System.StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.StockFilter = normalizedFilter;
            ViewBag.CategoryFilter = normalizedCategory;
            ViewBag.Search = search;

            return View(filtered.ToList());
        }

        // อัปเดตสต็อกรวดเร็ว (คงสถานะ Filter เดิมไว้)
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int id, int adjustment, string? stockFilter = null, string? categoryFilter = null, string? search = null, string? role = null)
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
            return RedirectToAction(nameof(Index), new { stockFilter, categoryFilter, search, role });
        }

        // หน้าเพิ่ม/แก้ไขสินค้าแบบ GET (รองรับเมื่อเข้าผ่าน URL หรือรีเฟรชหน้าจอ) -> นำทางไปยังหน้าคลังและเปิด Modal เพิ่มสินค้าทันที
        [HttpGet]
        public IActionResult SaveProduct(int? id = null, string? role = null)
        {
            return RedirectToAction(nameof(Index), new { role, openCreate = id == null ? "true" : null, editId = id });
        }

        [HttpGet]
        public IActionResult CreateProduct(string? role = null)
        {
            return RedirectToAction(nameof(Index), new { role, openCreate = "true" });
        }

        [HttpGet]
        public IActionResult AddProduct(string? role = null)
        {
            return RedirectToAction(nameof(Index), new { role, openCreate = "true" });
        }

        // เพิ่มหรือแก้ไขสินค้า (Req 6, 17) รองรับอัปโหลดรูปภาพและลิงก์รูปภาพ
        [HttpPost]
        public async Task<IActionResult> SaveProduct(Product model, IFormFile? imageFile, string? role = null)
        {
            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string webRoot = !string.IsNullOrEmpty(_env.WebRootPath)
                        ? _env.WebRootPath
                        : System.IO.Path.Combine(_env.ContentRootPath, "wwwroot");

                    var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(imageFile, webRoot, "products");
                    if (uploadErr != null)
                    {
                        TempData["ErrorMessage"] = uploadErr;
                        return RedirectToAction(nameof(Index), new { role, openCreate = model.Id == 0 ? "true" : null, editId = model.Id > 0 ? (int?)model.Id : null });
                    }
                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        model.ImageUrl = uploadedUrl;
                    }
                }

                if (_repo.SaveProduct(model, out var message))
                {
                    TempData["SuccessMessage"] = message;

                    try
                    {
                        // SignalR Real-Time Socket Broadcast
                        await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", model.Id, model.StockQuantity);
                        if (model.IsLowStock)
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", model.Name, model.StockQuantity);
                        }
                    }
                    catch
                    {
                        // ป้องกัน SignalR error ส่งผลกระทบต่อการบันทึกสินค้า
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการบันทึกข้อมูลสินค้า: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { role });
        }

        [HttpGet]
        public IActionResult DeleteProduct(string? role = null)
        {
            return RedirectToAction(nameof(Index), new { role });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id, string? stockFilter = null, string? categoryFilter = null, string? search = null, string? role = null)
        {
            try
            {
                var (success, message) = _repo.DeleteProduct(id);
                if (success)
                {
                    TempData["SuccessMessage"] = message;
                    try
                    {
                        // SignalR Real-Time Socket Broadcast
                        await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", id, 0);
                    }
                    catch { }
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการลบสินค้า: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { stockFilter, categoryFilter, search, role });
        }
    }
}
