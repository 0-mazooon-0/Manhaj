using Manhaj.IRepositories;
using Manhaj.Models;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace Manhaj.Services
{
    public class StudentService : IStudentService
    {
        private readonly ManhajDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ICourseRepository _courseRepository;

        public StudentService(ManhajDbContext context, IConfiguration configuration, ICourseRepository courseRepository)
        {
            _context = context;
            _configuration = configuration;
            _courseRepository = courseRepository;
        }

        public async Task<Session> CreateCheckoutSessionAsync(int courseId, int userId, string domain)
        {
            // Check if already enrolled
            var existingEnrollment = await _context.Course_Registrations
                .FirstOrDefaultAsync(cr => cr.StudentId == userId && cr.CourseId == courseId);

            if (existingEnrollment != null && existingEnrollment.IsApproved)
            {
                // Already approved
                return null;
            }

            var course = _courseRepository.GetCourseWithDetails(courseId);
            if (course == null) return null;

            Stripe.StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(course.Price * 100), // Amount in cents
                            Currency = "egp",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = course.Title,
                                Description = course.Description,
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = domain + $"/Student/PaymentSuccess?session_id={{CHECKOUT_SESSION_ID}}&courseId={courseId}",
                CancelUrl = domain + $"/Student/PaymentCancel?courseId={courseId}",
                Metadata = new Dictionary<string, string>
                {
                    { "courseId", courseId.ToString() },
                    { "userId", userId.ToString() }
                }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);
            return session;
        }

        public async Task<bool> ProcessPaymentSuccessAsync(string sessionId, int courseId, int userId)
        {
            Stripe.StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            if (session.PaymentStatus == "paid")
            {
                var existingEnrollment = await _context.Course_Registrations
                    .FirstOrDefaultAsync(cr => cr.StudentId == userId && cr.CourseId == courseId);

                if (existingEnrollment == null)
                {
                    var enrollment = new Course_Registration
                    {
                        StudentId = userId,
                        CourseId = courseId,
                        RegistrationDate = DateTime.Now,
                        EnrollmentDate = DateTime.Now,
                        Progress = 0,
                        Grade = 0,
                        IsApproved = true,
                        PaymentStatus = "Paid",
                        StripeSessionId = session.Id,
                        PaymentIntentId = session.PaymentIntentId
                    };
                    _context.Course_Registrations.Add(enrollment);
                }
                else
                {
                    existingEnrollment.IsApproved = true;
                    existingEnrollment.PaymentStatus = "Paid";
                    existingEnrollment.EnrollmentDate = DateTime.Now;
                    existingEnrollment.StripeSessionId = session.Id;
                    existingEnrollment.PaymentIntentId = session.PaymentIntentId;
                    _context.Course_Registrations.Update(existingEnrollment);
                }

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
