using System.ComponentModel.DataAnnotations;

namespace Manhaj.DTOs
{
    public class CourseVM
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public string Level { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public double Duration { get; set; }
        [Required]
        public int NumberOfLectures { get; set; }
        [Required]
        public DateTime CreationDate { get; set; }
        [Required]
        public int TeacherId { get; set; }
        
    }
}
