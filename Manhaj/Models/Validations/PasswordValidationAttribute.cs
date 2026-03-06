using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Manhaj.Models.Validation
{
    public class PasswordValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string ?? "";
            var errors = new List<string>();

            if (password.Length < 8)
                errors.Add("كلمة المرور على الأقل 8 أحرف");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                errors.Add("لابد من وجود حرف واحد كبير على الأقل");

            if (!Regex.IsMatch(password, @"[a-z]"))
                errors.Add("لابد من وجود حرف واحد صغير على الأقل");

            if (!Regex.IsMatch(password, @"[0-9]"))
                errors.Add("لابد من وجود رقم واحد على الاقل");

            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?{}|<>]"))
                errors.Add("لابد من وجود علامة خاصة واحددة على الأقل");

            if (errors.Any())
                return new ValidationResult(string.Join(", ", errors));

            return ValidationResult.Success;
        }
    }
}
