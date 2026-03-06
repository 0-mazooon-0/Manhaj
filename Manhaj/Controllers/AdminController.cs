using Manhaj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Manhaj.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ManhajDbContext _context;
        private readonly Manhaj.IRepositories.IStudentRepository _studentRepository;
        private readonly Manhaj.Services.IWhatsAppService _whatsAppService;

        public AdminController(ManhajDbContext context, Manhaj.IRepositories.IStudentRepository studentRepository, Manhaj.Services.IWhatsAppService whatsAppService)
        {
            _context = context;
            _studentRepository = studentRepository;
            _whatsAppService = whatsAppService;
        }

        // Helper method to set badge counts
        private async Task SetBadgeCounts()
        {
            ViewBag.PendingTeacherCount = await _context.Teachers.CountAsync(t => !t.IsApproved);
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            await SetBadgeCounts();

            ViewBag.PendingTeachers = ViewBag.PendingTeacherCount;

            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.TotalTeachers = await _context.Teachers.CountAsync();
            ViewBag.TotalCourses = await _context.Courses.CountAsync();

            ViewBag.RecentCourses = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Course_Registrations)
                .OrderByDescending(c => c.CreationDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentStudents = await _context.Students
                .OrderByDescending(s => s.Id)
                .Take(10)
                .ToListAsync();

            // Recent Pending Activities
            ViewBag.RecentPendingTeachers = await _context.Teachers
                .Where(t => !t.IsApproved)
                .OrderByDescending(t => t.Id) // Assuming Id roughly correlates to time, or add CreatedAt if available
                .Take(5)
                .ToListAsync();

            return View();
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            await SetBadgeCounts();
            var students = await _context.Students.ToListAsync();
            var teachers = await _context.Teachers
                .Include(t => t.Courses)
                .ToListAsync();
            
            ViewBag.Students = students;
            ViewBag.Teachers = teachers;
            
            return View();
        }

        // GET: /Admin/Courses
        public async Task<IActionResult> Courses()
        {
            await SetBadgeCounts();
            var courses = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lectures)
                .Include(c => c.Course_Registrations)
                .OrderByDescending(c => c.CreationDate)
                .ToListAsync();

            return View(courses);
        }

        // GET: /Admin/PendingTeachers
        public async Task<IActionResult> PendingTeachers()
        {
            await SetBadgeCounts();
            var teachers = await _context.Teachers
                .Where(t => !t.IsApproved)
                .ToListAsync();

            return View(teachers);
        }

        // POST: /Admin/ApproveTeacher/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            
            if (teacher == null)
            {
                return NotFound();
            }

            teacher.IsApproved = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تمت الموافقة على المعلم بنجاح";
            return RedirectToAction("PendingTeachers");
        }

        // POST: /Admin/RejectTeacher/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            
            if (teacher == null)
            {
                return NotFound();
            }

            // Add to Blacklist
            var blacklistEntry = new Blacklist
            {
                Email = teacher.Email,
                Reason = "Rejected Teacher Application",
                AddedDate = DateTime.Now
            };
            _context.Blacklist.Add(blacklistEntry);

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم رفض طلب المعلم وإضافته للقائمة السوداء";
            return RedirectToAction("PendingTeachers");
        }

        // GET: /Admin/Blacklist
        public async Task<IActionResult> Blacklist()
        {
            await SetBadgeCounts();
            var blacklist = await _context.Blacklist
                .OrderByDescending(b => b.AddedDate)
                .ToListAsync();
            return View(blacklist);
        }

        // POST: /Admin/RemoveFromBlacklist/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromBlacklist(int id)
        {
            var entry = await _context.Blacklist.FindAsync(id);
            if (entry != null)
            {
                _context.Blacklist.Remove(entry);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تمت إزالة البريد الإلكتروني من القائمة السوداء";
            }
            return RedirectToAction("Blacklist");
        }




        // POST: /Admin/DeleteTeacher/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.Courses)
                    .ThenInclude(c => c.Course_Registrations)
                .Include(t => t.Courses)
                    .ThenInclude(c => c.Lectures)
                        .ThenInclude(l => l.Quiz)
                .Include(t => t.Courses)
                    .ThenInclude(c => c.Lectures)
                        .ThenInclude(l => l.Assignment)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            // Add to Blacklist
            var blacklistEntry = new Blacklist
            {
                Email = teacher.Email,
                Reason = "Deleted by Admin",
                AddedDate = DateTime.Now
            };
            _context.Blacklist.Add(blacklistEntry);

            // Cleanup related data for each course
            foreach (var course in teacher.Courses)
            {
                // 1. Remove Course Registrations
                _context.Course_Registrations.RemoveRange(course.Course_Registrations);
                

                // 3. Remove Student Progress (Lectures)
                var lectureIds = course.Lectures.Select(l => l.Id).ToList();
                var studentLectures = await _context.Student_Lectures
                    .Where(sl => lectureIds.Contains(sl.LectureId))
                    .ToListAsync();
                _context.Student_Lectures.RemoveRange(studentLectures);

                // 4. Remove Quizzes and related data
                var quizzes = course.Lectures.Where(l => l.Quiz != null).Select(l => l.Quiz).ToList();
                if (quizzes.Any())
                {
                    var quizIds = quizzes.Select(q => q.Id).ToList();
                    
                    // Remove Student Quiz Answers
                    var studentQuizAnswers = await _context.Student_Quiz_Answers
                        .Where(sqa => quizIds.Contains(sqa.QuizId))
                        .ToListAsync();
                    _context.Student_Quiz_Answers.RemoveRange(studentQuizAnswers);

                    // Remove Student Quizzes (Attempts)
                    var studentQuizzes = await _context.Student_Quizzes
                        .Where(sq => quizIds.Contains(sq.QuizId))
                        .ToListAsync();
                    _context.Student_Quizzes.RemoveRange(studentQuizzes);

                    // Remove Quizzes
                    _context.Quizzes.RemoveRange(quizzes);
                }

                // 5. Remove Assignments and related data
                var assignments = course.Lectures.Where(l => l.Assignment != null).Select(l => l.Assignment).ToList();
                if (assignments.Any())
                {
                    var assignmentIds = assignments.Select(a => a.Id).ToList();

                    // Remove Student Assignments
                    var studentAssignments = await _context.Student_Assignments
                        .Where(sa => assignmentIds.Contains(sa.AssignmentId))
                        .ToListAsync();
                    _context.Student_Assignments.RemoveRange(studentAssignments);

                    // Remove Assignments
                    _context.Assignments.RemoveRange(assignments);
                }
            }

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف المعلم وإضافته للقائمة السوداء";
            return RedirectToAction("Users");
        }

        // POST: /Admin/DeleteStudent/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students
                .Include(s => s.Course_Registrations)
                .Include(s => s.Quizzes)
                .Include(s => s.Assignments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            // Add to Blacklist
            var blacklistEntry = new Blacklist
            {
                Email = student.Email,
                Reason = "Deleted by Admin",
                AddedDate = DateTime.Now
            };
            _context.Blacklist.Add(blacklistEntry);

            // Manually remove related data if Restrict behavior is set
            _context.Course_Registrations.RemoveRange(student.Course_Registrations);
            _context.Student_Quizzes.RemoveRange(student.Quizzes);
            _context.Student_Assignments.RemoveRange(student.Assignments);
            
            // Remove Student Lectures (Progress)
            var studentLectures = await _context.Student_Lectures
                .Where(sl => sl.StudentId == id)
                .ToListAsync();
            _context.Student_Lectures.RemoveRange(studentLectures);

            // Remove Student Quiz Answers
            var studentQuizAnswers = await _context.Student_Quiz_Answers
                .Where(sqa => sqa.StudentId == id)
                .ToListAsync();
            _context.Student_Quiz_Answers.RemoveRange(studentQuizAnswers);

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الطالب وإضافته للقائمة السوداء";
            return RedirectToAction("Users");
        }

        // POST: /Admin/DeleteCourse/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Course_Registrations)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Quiz)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Assignment)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            // Manually remove related data if Restrict behavior is set
            _context.Course_Registrations.RemoveRange(course.Course_Registrations);
            
            // Remove Student Progress (Lectures)
            var lectureIds = course.Lectures.Select(l => l.Id).ToList();
            var studentLectures = await _context.Student_Lectures
                .Where(sl => lectureIds.Contains(sl.LectureId))
                .ToListAsync();
            _context.Student_Lectures.RemoveRange(studentLectures);

            // Remove Quizzes and related data
            var quizzes = course.Lectures.Where(l => l.Quiz != null).Select(l => l.Quiz).ToList();
            if (quizzes.Any())
            {
                var quizIds = quizzes.Select(q => q.Id).ToList();
                
                // Remove Student Quiz Answers
                var studentQuizAnswers = await _context.Student_Quiz_Answers
                    .Where(sqa => quizIds.Contains(sqa.QuizId))
                    .ToListAsync();
                _context.Student_Quiz_Answers.RemoveRange(studentQuizAnswers);

                // Remove Student Quizzes (Attempts)
                var studentQuizzes = await _context.Student_Quizzes
                    .Where(sq => quizIds.Contains(sq.QuizId))
                    .ToListAsync();
                _context.Student_Quizzes.RemoveRange(studentQuizzes);

                // Remove Quizzes
                _context.Quizzes.RemoveRange(quizzes);
            }

            // Remove Assignments and related data
            var assignments = course.Lectures.Where(l => l.Assignment != null).Select(l => l.Assignment).ToList();
            if (assignments.Any())
            {
                var assignmentIds = assignments.Select(a => a.Id).ToList();

                // Remove Student Assignments
                var studentAssignments = await _context.Student_Assignments
                    .Where(sa => assignmentIds.Contains(sa.AssignmentId))
                    .ToListAsync();
                _context.Student_Assignments.RemoveRange(studentAssignments);

                // Remove Assignments
                _context.Assignments.RemoveRange(assignments);
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الدورة بنجاح";
            return RedirectToAction("Courses");
        }
        // POST: /Admin/SendParentReport/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendParentReport(int id)
        {
            var student = _studentRepository.GetStudentWithDetails(id);
            if (student == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(student.ParentPhoneNumber))
            {
                TempData["ErrorMessage"] = "رقم ولي الأمر غير مسجل لهذا الطالب";
                return RedirectToAction("Users");
            }

            var reportBuilder = new System.Text.StringBuilder();
            reportBuilder.AppendLine($"الاسم: {student.FirstName} {student.LastName}");
            reportBuilder.AppendLine($"الصف: {student.Level}");

            // Calculate stats across all enrolled courses
            int totalLectures = 0;
            int finishedLectures = 0;
            
            decimal totalQuizGrade = 0;
            decimal totalQuizMaxGrade = 0;
            
            decimal totalAssignmentGrade = 0;
            decimal totalAssignmentMaxGrade = 0;

            foreach (var registration in student.Course_Registrations)
            {
                var course = registration.Course;
                
                // Lectures
                var courseLectures = course.Lectures;
                totalLectures += courseLectures.Count;
                
                if (student.Student_Lectures != null)
                {
                    var courseLectureIds = courseLectures.Select(l => l.Id).ToList();
                    finishedLectures += student.Student_Lectures.Count(sl => courseLectureIds.Contains(sl.LectureId) && sl.IsCompleted);
                }

                // Quizzes
                var courseQuizzes = student.Quizzes.Where(q => q.Quiz.CourseId == course.Id || (q.Quiz.Lecture != null && q.Quiz.Lecture.CourseId == course.Id)).ToList();
                foreach (var quiz in courseQuizzes)
                {
                    totalQuizGrade += quiz.Grade;
                    totalQuizMaxGrade += quiz.Quiz.TotalGrade;
                }

                // Assignments
                var courseAssignments = student.Assignments.Where(a => a.Assignment.Lecture.CourseId == course.Id).ToList();
                foreach (var assignment in courseAssignments)
                {
                    totalAssignmentGrade += assignment.Grade;
                    totalAssignmentMaxGrade += assignment.Assignment.TotalGrade;
                }
            }

            reportBuilder.AppendLine($"عدد المحاضرات التي تم مشاهدتها: {finishedLectures}/{totalLectures}");
            
            string quizAverage = totalQuizMaxGrade > 0 ? $"{(totalQuizGrade / totalQuizMaxGrade * 100):F1}%" : "لا يوجد";
            reportBuilder.AppendLine($"متوسط درجات الاختبارات: {quizAverage}");

            string assignmentAverage = totalAssignmentMaxGrade > 0 ? $"{(totalAssignmentGrade / totalAssignmentMaxGrade * 100):F1}%" : "لا يوجد";
            reportBuilder.AppendLine($"متوسط درجات الواجبات: {assignmentAverage}");

            var message = reportBuilder.ToString();
            var success = await _whatsAppService.SendMessageAsync(student.ParentPhoneNumber, message);

            if (success)
            {
                TempData["SuccessMessage"] = "تم إرسال التقرير لولي الأمر بنجاح";
            }
            else
            {
                TempData["ErrorMessage"] = "فشل إرسال التقرير. تأكد من إعدادات واتساب ورقم الهاتف";
            }

            return RedirectToAction("Users");
        }
    }
}
