using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manhaj.Models
{
    public class Course_Registration
    {
        [Required]
        public DateTime RegistrationDate { get; set; }
        public DateTime? EnrollmentDate { get; set; }

        public decimal Grade { get; set; }
        [Required]
        [Range(1, 100)]
        public int Progress { get; set; }

        public bool IsApproved { get; set; } = false;
        public string? PaymentProofPath { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string? StripeSessionId { get; set; }
        public string? PaymentIntentId { get; set; }

        public virtual Course Course { get; set; }
        public int CourseId { get; set; }

        public virtual Student Student { get; set; }
        public int StudentId { get; set; }
    }
}
