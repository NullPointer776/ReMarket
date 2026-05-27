using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ReMarket.Models.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PostalCodeAttribute : ValidationAttribute
    {
        private static readonly Regex Pattern = new(
            @"^[A-Za-z0-9\s\-]{3,20}$",
            RegexOptions.Compiled);

        public PostalCodeAttribute()
            : base("Please enter a valid postal code.")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string text || string.IsNullOrWhiteSpace(text))
                return ValidationResult.Success;

            return Pattern.IsMatch(text.Trim())
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage);
        }
    }
}
