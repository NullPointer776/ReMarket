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

    public static class ItemImageUpload
    {
        public static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
        public const long MaxBytes = 5 * 1024 * 1024;

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

        // Validates number of files and each file (size, extension).
        public static string? ValidateImageFiles(IFormFile[]? files, bool requireAtLeastOne, int maxFiles = 8)
        {
            var list = files == null
                ? new List<IFormFile>()
                : files.Where(f => f != null && f.Length > 0).Cast<IFormFile>().ToList();

            if (requireAtLeastOne && list.Count == 0)
                return "Please select at least one image.";

            if (list.Count > maxFiles)
                return $"You can upload at most {maxFiles} images.";

            foreach (var f in list)
            {
                var err = Validate(f, required: false);
                if (err != null) return err;
            }

            return null;
        }
        
        public static async Task<string> SaveAsync(IWebHostEnvironment env, IFormFile file, string slugBase, int imageIndex = 0)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                ext = ".jpg";

            var dir = Path.Combine(env.WebRootPath, "images", "items");
            Directory.CreateDirectory(dir);

            var suffix = imageIndex <= 0 ? "" : $"-{imageIndex + 1}";
            var name = $"{slugBase}{suffix}{ext}";
            var path = Path.Combine(dir, name);

            await using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/images/items/" + name;
        }
    }
}
