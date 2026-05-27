using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ReMarket.Models.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PhoneNumberAttribute : ValidationAttribute
    {
        private static readonly Regex Pattern = new(
            @"^[\d\s+\-().]{6,30}$",
            RegexOptions.Compiled);

        public PhoneNumberAttribute()
            : base("Please enter a valid phone number.")
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
