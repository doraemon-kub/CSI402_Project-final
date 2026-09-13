using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Models;
using System;
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
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            var query = _repo.SupportTickets.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            ViewBag.SelectedStatus = status;
            ViewBag.NewCount = _repo.SupportTickets.Count(t => t.Status == TicketStatus.New);
            ViewBag.InvestigatingCount = _repo.SupportTickets.Count(t => t.Status == TicketStatus.Investigating);
            ViewBag.ResolvedCount = _repo.SupportTickets.Count(t => t.Status == TicketStatus.Resolved);

            return View(query.OrderByDescending(t => t.CreatedAt).ToList());
        }

        // บันทึกรายละเอียดการแก้ไขปัญหาและปิดงาน (Req 24)
        [HttpPost]
        public IActionResult ResolveTicket(int ticketId, string technician, string resolutionNote, TicketStatus newStatus)
        {
            var ticket = _repo.SupportTickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket != null)
            {
                ticket.AssignedTechnician = technician;
                ticket.ResolutionDetails = resolutionNote;
                ticket.Status = newStatus;
                if (newStatus == TicketStatus.Resolved || newStatus == TicketStatus.Closed)
                {
                    ticket.ResolvedAt = DateTime.Now;
                }
                TempData["SuccessMessage"] = $"บันทึกประวัติการแก้ไขและอัปเดตสถานะ Ticket #{ticket.TicketCode} เรียบร้อยแล้ว";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
