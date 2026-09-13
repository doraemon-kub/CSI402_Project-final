using System;
using System.Collections.Generic;

namespace CSI402_Project_final.Models
{
    public enum OrderStatus
    {
        PendingPayment,     // รอชำระเงิน
        PaymentVerified,    // ชำระเงินเรียบร้อย รอจัดของ (Order Staff)
        Preparing,          // กำลังจัดเตรียมและแพ็กสินค้า
        Shipped,            // จัดส่งแล้ว มีเลขพัสดุ (Shipping Staff)
        Completed,          // ลูกค้าได้รับสินค้าแล้ว
        Cancelled,          // ยกเลิกคำสั่งซื้อ
        RefundRequested,    // ลูกค้าขอคืนเงิน
        Refunded            // คืนเงินเรียบร้อยแล้ว (Accounting Staff)
    }

    public class OrderItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public double UnitWeightKg { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
        public double TotalWeightKg => UnitWeightKg * Quantity;
    }

    public class Order
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public List<OrderItem> Items { get; set; } = new();
        public double EstimatedWeightKg { get; set; }
        public double? ActualWeightKg { get; set; } // น้ำหนักชั่งจริงโดย Shipping Staff

        public decimal ItemsTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }

        public string? FreeGiftItem { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.PaymentVerified;

        // ข้อมูลขนส่ง (Shipping Staff)
        public string Courier { get; set; } = "Flash Express";
        public string? TrackingNumber { get; set; }
        public DateTime? ShippedDate { get; set; }

        // ข้อมูลการเงิน (Accounting Staff)
        public string PaymentMethod { get; set; } = "โอนผ่านธนาคาร";
        public string? PaymentSlipUrl { get; set; }
        public bool IsPaymentApproved { get; set; } = true;
        public decimal? RefundAmount { get; set; }
        public string? RefundReason { get; set; }
        public DateTime? RefundedDate { get; set; }
    }
}
