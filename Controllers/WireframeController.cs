using Microsoft.AspNetCore.Mvc;

namespace CSI402_Project_final.Controllers
{
    public class WireframeController : Controller
    {
        // หน้ารวม Wireframe ทุกหน้าของระบบ สำหรับส่งอาจารย์วิชา CSI402
        public IActionResult Index(string view = "home")
        {
            ViewBag.ActiveView = view.ToLower();
            return View();
        }
    }
}
