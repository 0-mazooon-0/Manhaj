using Manhaj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Manhaj.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Manhaj.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IProgressRepository _progressRepository;
        private readonly ManhajDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly Manhaj.Services.IStudentService _studentService;

        public StudentController(IStudentRepository studentRepository, ICourseRepository courseRepository, IProgressRepository progressRepository, ManhajDbContext context, IConfiguration configuration, Manhaj.Services.IStudentService studentService)
        {
            _studentRepository = studentRepository;
            _courseRepository = courseRepository;
            _progressRepository = progressRepository;
            _context = context;
            _configuration = configuration;
            _studentService = studentService;
        }



        // GET: /Student/Dashboard
        public IActionResult Dashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var student = _studentRepository.GetStudentWithDetails(userId);

            if (student == null)
            {
                return NotFound();
            }

            // Calculate statistics
            var enrolledCourses = _studentRepository.GetEnrolledCourses(userId) ?? new List<Course_Registration>();
            ViewBag.TotalEnrolledCourses = enrolledCourses.Count;
            
            // Completed courses (100% progress)
            ViewBag.CompletedCourses = enrolledCourses.Count(c => 
                _progressRepository.GetStudentProgress(userId, c.CourseId) == 100);
            
            // Total completed lectures
            ViewBag.CompletedLectures = enrolledCourses.Sum(c => 
                _progressRepository.GetCompletedLectures(userId, c.CourseId)?.Count ?? 0);
            
            // Average quiz score - simplified
            // Average quiz score - simplified
            ViewBag.AverageQuizScore = 0m;

            // Calculate Total Quizzes and Assignments
            int totalQuizzes = 0;
            int totalAssignments = 0;
            int completedQuizzes = student.Quizzes?.Count ?? 0;
            int completedAssignments = student.Assignments?.Count ?? 0;

            if (student.Course_Registrations != null)
            {
                foreach (var reg in student.Course_Registrations)
                {
                    if (reg.Course?.Lectures != null)
                    {
                        totalQuizzes += reg.Course.Lectures.Count(l => l.Quiz != null);
                        totalAssignments += reg.Course.Lectures.Count(l => l.Assignment != null);
                    }
                }
            }
            
            ViewBag.TotalQuizzes = totalQuizzes;
            ViewBag.CompletedQuizzes = completedQuizzes;
            ViewBag.TotalAssignments = totalAssignments;
            ViewBag.CompletedAssignments = completedAssignments;

            // Recent Activities - Merged and Sorted
            var activities = new List<Manhaj.ViewModels.ActivityVM>();

            var upcomingQuizzes = _studentRepository.GetUpcomingQuizzes(userId);
            foreach (var q in upcomingQuizzes)
            {
                activities.Add(new Manhaj.ViewModels.ActivityVM
                {
                    Type = "Quiz",
                    Title = q.Title,
                    CourseTitle = q.Course?.Title ?? q.Lecture?.Course?.Title ?? "غير محدد",
                    Date = q.Deadline ?? q.StartTime.AddMinutes(q.Duration), // Use Deadline or calc end time
                    Link = Url.Action("Details", "Quiz", new { id = q.Id }),
                    LinkText = "بدء الاختبار"
                });
            }

            var upcomingAssignments = _studentRepository.GetUpcomingAssignments(userId);
            foreach (var a in upcomingAssignments)
            {
                activities.Add(new Manhaj.ViewModels.ActivityVM
                {
                    Type = "Assignment",
                    Title = a.Title,
                    CourseTitle = a.Lecture?.Course?.Title ?? "غير محدد",
                    Date = a.Deadline,
                    Link = Url.Action("Details", "Assignment", new { id = a.Id }),
                    LinkText = "عرض الواجب"
                });
            }

            var recentLectures = _studentRepository.GetRecentLectures(userId);
            foreach (var l in recentLectures)
            {
                activities.Add(new Manhaj.ViewModels.ActivityVM
                {
                    Type = "Lecture",
                    Title = l.Title,
                    CourseTitle = l.Course?.Title ?? "غير محدد",
                    Date = l.CreatedAt,
                    Link = l.VideoUrl,
                    LinkText = "مشاهدة"
                });
            }

            // Sort by Date Descending (Most recent/upcoming first)
            // Or maybe separate logic? User said "recent in the top". 
            // If we mix future deadlines and past created dates, "most recent" usually means "closest to now" or "newest created".
            // For a feed, usually "Newest Created" is best. But Quizzes have Deadlines.
            // Let's sort by Date Descending. Future dates (deadlines) will be at top, then recent lectures.
            ViewBag.Activities = activities.OrderByDescending(a => a.Date).ToList();

            return View(student);
        }

        // GET: /Student/Courses
        public IActionResult Courses()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var enrolledCourses = _studentRepository.GetEnrolledCourses(userId);

            return View(enrolledCourses);
        }

        // GET: /Student/Course/{id}
        public IActionResult Course(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Check if student is enrolled
            var enrolledCourses = _studentRepository.GetEnrolledCourses(userId);
            var enrollment = enrolledCourses.FirstOrDefault(cr => cr.CourseId == id);

            var course = _courseRepository.GetCourseWithDetails(id);

            if (course == null)
            {
                return NotFound();
            }

            // Fetch progress data
            if (enrollment != null)
            {
                ViewBag.EnrollmentStatus = enrollment.IsApproved ? "Approved" : enrollment.PaymentStatus == "Pending" ? "Pending" : "Rejected";
                
                if (enrollment.IsApproved)
                {
                    ViewBag.ProgressPercentage = _progressRepository.GetStudentProgress(userId, id);
                    ViewBag.CompletedLectures = _progressRepository.GetCompletedLectures(userId, id);
                }
                else
                {
                    ViewBag.ProgressPercentage = 0;
                    ViewBag.CompletedLectures = new List<int>();
                }
            }
            else
            {
                ViewBag.EnrollmentStatus = "None";
                ViewBag.ProgressPercentage = 0;
                ViewBag.CompletedLectures = new List<int>();
            }

            return View(course);
        }

        // GET: /Student/BrowseCourses
        public IActionResult BrowseCourses()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var student = _studentRepository.GetStudentWithDetails(userId);

            var courses = _courseRepository.GetAllCoursesWithDetails()
                .Where(c => c.Level == student.Level)
                .Select(c => new Course
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Level = c.Level,
                    Price = c.Price,
                    Teacher = c.Teacher,
                    Lectures = c.Lectures,
                    Course_Registrations = c.Course_Registrations ?? new List<Course_Registration>()
                })
                .ToList();

            return View(courses);
        }

        // POST: /Student/Enroll/{courseId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Enroll(int courseId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Check if already enrolled
            var enrolledCourses = _studentRepository.GetEnrolledCourses(userId);
            var existingEnrollment = enrolledCourses.FirstOrDefault(cr => cr.CourseId == courseId);
            
            if (existingEnrollment != null)
            {
                if (existingEnrollment.IsApproved)
                {
                    TempData["ErrorMessage"] = "أنت مسجل بالفعل في هذه الدورة";
                }
                else if (existingEnrollment.PaymentStatus == "Pending")
                {
                    TempData["ErrorMessage"] = "لديك طلب تسجيل معلق لهذه الدورة";
                }
                else
                {
                    // If rejected, maybe allow re-enrollment? For now just show message
                    TempData["ErrorMessage"] = "تم رفض طلب تسجيلك السابق. يرجى التواصل مع الإدارة";
                }
                return RedirectToAction("Course", new { id = courseId });
            }

            var success = _studentRepository.RegisterCourse(userId, courseId);
            
            if (success)
            {
                TempData["SuccessMessage"] = "تم التسجيل في الدورة بنجاح!";
            }
            else
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء التسجيل";
            }

            return RedirectToAction("Course", new { id = courseId });
        }

        [HttpPost]
        public IActionResult ToggleLectureCompletion(int lectureId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var success = _progressRepository.ToggleProgress(userId, lectureId);
            
            if (success)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Failed to update progress" });
        }

        // GET: /Student/PurchaseCourse/{courseId}
        public IActionResult PurchaseCourse(int courseId)
        {
            var course = _courseRepository.GetCourseWithDetails(courseId);
            
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: /Student/PurchaseCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("PurchaseCourse")]
        public async Task<IActionResult> PurchaseCourseProcess(int courseId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var domain = $"{Request.Scheme}://{Request.Host}";
            
            var session = await _studentService.CreateCheckoutSessionAsync(courseId, userId, domain);
            
            if (session == null)
            {
                TempData["ErrorMessage"] = "حدث خطأ أثناء إنشاء جلسة الدفع أو أنك مسجل بالفعل في هذه الدورة";
                return RedirectToAction("Course", new { id = courseId });
            }

            return Redirect(session.Url);
        }

        public async Task<IActionResult> PaymentSuccess(string session_id, int courseId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var success = await _studentService.ProcessPaymentSuccessAsync(session_id, courseId, userId);

            if (success)
            {
                TempData["SuccessMessage"] = "تم الدفع والتسجيل بنجاح!";
                return RedirectToAction("Course", new { id = courseId });
            }

            TempData["ErrorMessage"] = "لم يتم الدفع بنجاح.";
            return RedirectToAction("Course", new { id = courseId });
        }

        public IActionResult PaymentCancel(int courseId)
        {
            TempData["ErrorMessage"] = "تم إلغاء عملية الدفع.";
            return RedirectToAction("PurchaseCourse", new { courseId = courseId });
        }

    }
}
