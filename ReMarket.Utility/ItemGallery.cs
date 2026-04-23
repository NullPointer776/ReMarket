using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ReMarket.Models;

namespace ReMarket.Utility
{
    // Combines cover image + extra URLs stored as JSON (max 8 total).
    public static class ItemGallery
    {
        public const int MaxImages = 8;

        public static IReadOnlyList<string> GetAllImageUrls(Item item)
        {
            var urls = new List<string>();
            if (!string.IsNullOrEmpty(item.ImageUrl))
                urls.Add(item.ImageUrl);

            if (string.IsNullOrEmpty(item.MoreImageUrlsJson))
                return urls;

            try
            {
                var more = JsonSerializer.Deserialize<List<string>>(item.MoreImageUrlsJson);
                if (more == null) return urls;
                foreach (var u in more)
                {
                    if (string.IsNullOrWhiteSpace(u)) continue;
                    if (!urls.Contains(u, StringComparer.OrdinalIgnoreCase))
                        urls.Add(u);
                }
            }
            catch
            {
                /* ignore bad json */
            }

            return urls.Take(MaxImages).ToList();
        }

        public static void SetGalleryFromUrls(Item item, IReadOnlyList<string> orderedUrls)
        {
            var list = orderedUrls.Where(u => !string.IsNullOrWhiteSpace(u))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaxImages)
                .ToList();

            if (list.Count == 0)
            {
                item.ImageUrl = null;
                item.MoreImageUrlsJson = null;
                return;
            }

            item.ImageUrl = list[0];
            item.MoreImageUrlsJson = list.Count > 1
                ? JsonSerializer.Serialize(list.Skip(1).ToList())
                : null;
        }
    }
}
