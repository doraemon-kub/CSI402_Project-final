using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Models;
using System.Linq;

namespace CSI402_Project_final.Controllers
{
    [Authorize(Roles = "ITSupport,Owner,Admin")]
    public class SupportController : Controller
    {
        private readonly PetShopRepository _repo;

        public SupportController(PetShopRepository repo)
        {
            _repo = repo;
        }

        // หน้า Helpdesk Board ของ IT Support (Req 23)
        public IActionResult Index(TicketStatus? status = null)
        {
            var allTickets = _repo.GetSupportTickets();
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.SelectedStatus = status;
            ViewBag.NewCount = allTickets.Count(t => t.Status == TicketStatus.New);
            ViewBag.InvestigatingCount = allTickets.Count(t => t.Status == TicketStatus.Investigating);
            ViewBag.ResolvedCount = allTickets.Count(t => t.Status == TicketStatus.Resolved);

            var filtered = status.HasValue
                ? allTickets.Where(t => t.Status == status.Value).ToList()
                : allTickets;

            return View(filtered);
        }

        // บันทึกรายละเอียดการแก้ไขปัญหาและปิดงาน (Req 24)
        [HttpPost]
        public IActionResult ResolveTicket(int ticketId, string technician, string resolutionNote, TicketStatus newStatus)
        {
            var ticket = _repo.ResolveSupportTicket(ticketId, technician, resolutionNote, newStatus);
            if (ticket != null)
            {
                TempData["SuccessMessage"] = $"บันทึกประวัติการแก้ไขและอัปเดตสถานะ Ticket #{ticket.TicketCode} เรียบร้อยแล้ว";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
