using System.Text.RegularExpressions;
using ReMarket.Models;

namespace ReMarket.Utility
{
    public static class InputSanitizer
    {
        private static readonly Regex HtmlTagPattern = new(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ScriptPattern = new(@"(?i)javascript\s*:|vbscript\s*:|<\s*/?\s*script", RegexOptions.Compiled);

        public const int MaxQueryLength = 100;

        public static string? CleanText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var cleaned = value.Trim().Replace("\0", string.Empty);
            cleaned = HtmlTagPattern.Replace(cleaned, string.Empty);
            cleaned = ScriptPattern.Replace(cleaned, string.Empty);
            return cleaned;
        }

        public static string? CleanQueryParam(string? value, int maxLength = MaxQueryLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var cleaned = CleanText(value);
            if (string.IsNullOrWhiteSpace(cleaned))
                return null;

            return cleaned.Length <= maxLength ? cleaned : cleaned[..maxLength];
        }

        public static bool ContainsUnsafeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.Contains('<') || value.Contains('>') || ScriptPattern.IsMatch(value);
        }

        public static void SanitizeItem(Item item)
        {
            item.Name = CleanText(item.Name) ?? string.Empty;
            item.Description = CleanText(item.Description);
            item.Location = CleanText(item.Location) ?? string.Empty;
            item.RejectionReason = CleanText(item.RejectionReason);
            if (!string.IsNullOrWhiteSpace(item.Slug))
                item.Slug = CleanText(item.Slug);
        }

        public static void SanitizeCategory(Category category)
        {
            category.Name = CleanText(category.Name) ?? string.Empty;
            category.Description = CleanText(category.Description);
            if (!string.IsNullOrWhiteSpace(category.Slug))
                category.Slug = CleanText(category.Slug);
        }

        public static void SanitizeUser(ApplicationUser user)
        {
            user.FirstName = CleanText(user.FirstName);
            user.LastName = CleanText(user.LastName);
            user.StreetAddress = CleanText(user.StreetAddress);
            user.Suburb = CleanText(user.Suburb);
            user.City = CleanText(user.City);
            user.PostalCode = CleanText(user.PostalCode);
            user.Country = CleanText(user.Country);
            user.State = CleanText(user.State);
        }

        public static void SanitizeOrderHeader(OrderHeader order)
        {
            order.Name = CleanText(order.Name) ?? string.Empty;
            order.PhoneNumber = CleanText(order.PhoneNumber) ?? string.Empty;
            order.StreetAddress = CleanText(order.StreetAddress) ?? string.Empty;
            order.City = CleanText(order.City) ?? string.Empty;
            order.State = CleanText(order.State) ?? string.Empty;
            order.PostalCode = CleanText(order.PostalCode) ?? string.Empty;
            order.Carrier = CleanText(order.Carrier);
            order.TrackingNumber = CleanText(order.TrackingNumber);
        }
    }
}
