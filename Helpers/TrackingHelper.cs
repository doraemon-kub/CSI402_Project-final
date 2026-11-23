using System;

namespace CSI402_Project_final.Helpers
{
    public static class TrackingHelper
    {
        private static readonly Random _rnd = new();

        public static string GenerateTrackingNumber(string? courier)
        {
            string c = (courier ?? "").ToLowerInvariant();
            if (c.Contains("kerry"))
            {
                return $"KRY{_rnd.Next(100000000, 999999999)}TH";
            }
            if (c.Contains("ไปรษณีย์") || c.Contains("ems") || c.Contains("post"))
            {
                return $"ED{_rnd.Next(100000000, 999999999)}TH";
            }
            // Flash Express format: TH + 12 digits + F
            return $"TH{_rnd.Next(100000, 999999)}{_rnd.Next(100000, 999999)}F";
        }
    }
}
