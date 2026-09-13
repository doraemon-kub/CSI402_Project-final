using System;

namespace CSI402_Project_final.Models
{
    public enum TicketStatus
    {
        New,            // เปิดเรื่องใหม่
        Investigating,  // กำลังตรวจสอบ
        Resolved,       // แก้ไขเสร็จสิ้น
        Closed          // ปิดเรื่อง
    }

    public enum TicketPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    public class SupportTicket
    {
        public int Id { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Category { get; set; } = "ทั่วไป"; // ระบบสั่งซื้อ, การชำระเงิน, โปรโมชั่น, บัญชีผู้ใช้
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        public TicketStatus Status { get; set; } = TicketStatus.New;

        // ฟังก์ชันบันทึกและประวัติการแก้ไขโดย IT Support (Req 23, 24)
        public string? AssignedTechnician { get; set; }
        public string? ResolutionDetails { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
