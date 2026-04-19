using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ReMarket.Utility
{
    /// <summary>
    /// Server-side validation and storage for item listing images under wwwroot/images/items.
    /// </summary>
    public static class ItemImageUpload
    {
        public static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
        public const long MaxBytes = 5 * 1024 * 1024;

        /// <summary>
        /// Returns an error message if invalid; null if valid. When <paramref name="required"/> is false, an empty upload is allowed.
        /// </summary>
        public static string? Validate(IFormFile? file, bool required)
        {
            if (file == null || file.Length == 0)
                return required ? "Please upload an item image." : null;

            if (file.Length > MaxBytes)
                return "Image must not exceed 5 MB.";

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                return "Only jpg, png, gif, or webp are allowed.";

            return null;
        }

        /// <summary>
        /// Saves the file to wwwroot/images/items/{slugBase}{ext} and returns the app-relative URL (e.g. /images/items/foo.jpg).
        /// </summary>
        public static async Task<string> SaveAsync(IWebHostEnvironment env, IFormFile file, string slugBase)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                ext = ".jpg";

            var dir = Path.Combine(env.WebRootPath, "images", "items");
            Directory.CreateDirectory(dir);

            var name = $"{slugBase}{ext}";
            var path = Path.Combine(dir, name);

            await using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/images/items/" + name;
        }
    }
}
