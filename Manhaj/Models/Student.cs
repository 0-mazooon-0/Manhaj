using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Manhaj.Models
{
    public class Student : User
    {
        [Required(ErrorMessage = "المستوى الدراسي مطلوب")]
        public String Level { get; set; }

        [Required(ErrorMessage = "رقم ولي الأمر مطلوب")]
        [RegularExpression(@"01(0|5|1|2)[0-9]{8}", ErrorMessage = "رقم الهاتف غير صحيح")]
        public string ParentPhoneNumber { get; set; }

        // Navigation property: one student → many teachers
        public virtual ICollection<Teacher> Teachers { get; set; }
        // One Student → Many Rating
        public virtual ICollection<Student_Quiz> Quizzes { get; set; }
        public virtual ICollection<Course_Registration> Course_Registrations { get; set; }
        public virtual ICollection<Student_Assignment> Assignments { get; set; }
        public virtual ICollection<Student_Lecture> Student_Lectures { get; set; }




    }
}
