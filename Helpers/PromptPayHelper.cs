using System;
using System.Globalization;
using System.Text;

namespace CSI402_Project_final.Helpers
{
    public static class PromptPayHelper
    {
        // สร้าง PromptPay EMVCo QR Payload พร้อมจำนวนเงินตามมาตรฐานธนาคารแห่งประเทศไทย
        public static string GeneratePayload(string mobileOrTaxId, decimal amount)
        {
            string sanitized = new string(Array.FindAll(mobileOrTaxId.ToCharArray(), char.IsDigit));
            string target;
            if (sanitized.Length == 10 && sanitized.StartsWith("0"))
            {
                // เบอร์มือถือไทย 08x -> 00668x (13 หลัก)
                target = "0066" + sanitized[1..];
            }
            else
            {
                target = sanitized;
            }

            string tag29Sub = FormatTlv("00", "A000000677010111") +
                              FormatTlv(sanitized.Length == 13 ? "02" : "01", target);

            var sb = new StringBuilder();
            sb.Append(FormatTlv("00", "01")); // Payload Format Indicator
            sb.Append(FormatTlv("01", amount > 0 ? "12" : "11")); // Dynamic if amount > 0
            sb.Append(FormatTlv("29", tag29Sub)); // Merchant Account Info (PromptPay)
            sb.Append(FormatTlv("53", "764")); // THB
            if (amount > 0)
            {
                sb.Append(FormatTlv("54", amount.ToString("0.00", CultureInfo.InvariantCulture)));
            }
            sb.Append(FormatTlv("58", "TH"));
            sb.Append("6304"); // CRC tag + length

            string crc = ComputeCrc16Ccitt(sb.ToString());
            sb.Append(crc);
            return sb.ToString();
        }

        private static string FormatTlv(string tag, string value)
        {
            return $"{tag}{value.Length:00}{value}";
        }

        private static string ComputeCrc16Ccitt(string input)
        {
            ushort crc = 0xFFFF;
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            foreach (byte b in bytes)
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                {
                    crc = (crc & 0x8000) != 0
                        ? (ushort)((crc << 1) ^ 0x1021)
                        : (ushort)(crc << 1);
                }
            }
            return crc.ToString("X4");
        }
    }
}

