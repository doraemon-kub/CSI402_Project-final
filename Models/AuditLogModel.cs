using System;

namespace CSI402_Project_final.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Username { get; set; } = "system";
        public string UserRole { get; set; } = "System";
        public string Action { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string? RecordId { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}

