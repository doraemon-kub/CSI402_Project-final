using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CSI402_Project_final.Helpers
{
    public static class FileUploadHelper
    {
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public static async Task<(string? Url, string? ErrorMessage)> SaveImageAsync(
            IFormFile? file,
            string webRootPath,
            string subfolder)
        {
            if (file == null || file.Length == 0)
            {
                return (null, null);
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (null, "ขนาดไฟล์รูปภาพต้องไม่เกิน 5 MB");
            }

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
            {
                return (null, "รองรับเฉพาะไฟล์รูปภาพ (.jpg, .jpeg, .png, .webp, .gif)");
            }

            string safeFolder = Path.Combine(webRootPath, "uploads", subfolder);
            if (!Directory.Exists(safeFolder))
            {
                Directory.CreateDirectory(safeFolder);
            }

            string fileName = $"{Guid.NewGuid():N}{ext}";
            string fullPath = Path.Combine(safeFolder, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return ($"/uploads/{subfolder}/{fileName}", null);
        }
    }
}

