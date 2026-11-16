using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSI402_Project_final.Models
{
    public class FinancialSummary
    {
        public int Id { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public int TotalOrdersCount { get; set; }
        public int SuccessfulOrdersCount { get; set; }
        public int RefundedOrdersCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public decimal TotalRefunds { get; set; }
        [NotMapped]
        public decimal ShippingCollected { get; set; }
        [NotMapped]
        public decimal ShippingCarrierCost { get; set; }

        public List<MonthlyFinancialEntry> MonthlyBreakdown { get; set; } = new();
    }

    public class MonthlyFinancialEntry
    {
        public int Id { get; set; }
        public int? FinancialSummaryId { get; set; }
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public decimal Profit => Revenue - Expenses;
    }
}
