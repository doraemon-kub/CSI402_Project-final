using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSI402_Project_final.Models
{
    public enum TicketStatus
    {
        New = 0,            // เปิดเรื่องใหม่
        Investigating = 1,  // กำลังตรวจสอบ
        Resolved = 2,       // แก้ไขเสร็จสิ้น
        Closed = 3          // ปิดเรื่อง
    }

    public enum TicketPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Urgent = 3
    }

    public class SupportTicket
    {
        public int Id { get; set; }

        [Column("TicketNumber")]
        public string TicketCode { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        [Column("CustomerEmail")]
        public string ContactEmail { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        [NotMapped]
        public string Category { get; set; } = "ทั่วไป"; // ไม่มีคอลัมน์ใน DB; ใช้ประกอบใน Subject/แสดงผล

        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
        public TicketStatus Status { get; set; } = TicketStatus.New;

        // ฟังก์ชันบันทึกและประวัติการแก้ไขโดย IT Support (Req 23, 24)
        public int? AssignedToStaffId { get; set; }

        [NotMapped]
        public string? AssignedTechnician { get; set; }

        [Column("StaffNotes")]
        public string? ResolutionDetails { get; set; }

        public string? RelatedOrderId { get; set; }

        [Column("ResolvedDate")]
        public DateTime? ResolvedAt { get; set; }
    }
}
