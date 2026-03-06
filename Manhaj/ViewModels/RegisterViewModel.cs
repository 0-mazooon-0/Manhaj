using System.ComponentModel.DataAnnotations;
using Manhaj.Models.Validation;
using Microsoft.AspNetCore.Http;

namespace Manhaj.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [StringLength(20, ErrorMessage = "الاسم الأول يجب ألا يتجاوز 20 حرف")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "الاسم الأخير مطلوب")]
        [StringLength(20, ErrorMessage = "الاسم الأخير يجب ألا يتجاوز 20 حرف")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [PasswordValidation(ErrorMessage = "كلمة المرور يجب أن تحتوي على حرف كبير، حرف صغير، رقم، وحرف خاص")]
        public string Password { get; set; }

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيد كلمة المرور غير متطابقتين")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [RegularExpression(@"01(0|5|1|2)[0-9]{8}", ErrorMessage = "رقم الهاتف غير صحيح")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "نوع المستخدم مطلوب")]
        public string UserType { get; set; } // "Student" or "Teacher"

        // For students
        public string? Level { get; set; }
        
        [RegularExpression(@"01(0|5|1|2)[0-9]{8}", ErrorMessage = "رقم الهاتف غير صحيح")]
        public string? ParentPhoneNumber { get; set; }

        // For teachers
        public string? Specialization { get; set; }
        public IFormFile? NationalIdFront { get; set; }
        public IFormFile? NationalIdBack { get; set; }
        public IFormFile? TeacherFormalCard { get; set; }
    }
}
