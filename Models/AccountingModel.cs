using System;
using System.Collections.Generic;

namespace CSI402_Project_final.Models
{
    public class FinancialSummary
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalRefunds { get; set; }
        public decimal ShippingCollected { get; set; }
        public decimal ShippingCarrierCost { get; set; }
        public decimal NetProfit => TotalRevenue - TotalExpenses - TotalRefunds;

        public List<MonthlyFinancialEntry> MonthlyBreakdown { get; set; } = new();
    }

    public class MonthlyFinancialEntry
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit => Revenue - Expenses;
    }
}
