using Stripe.Checkout;

namespace Manhaj.Services
{
    public interface IStudentService
    {
        Task<Session> CreateCheckoutSessionAsync(int courseId, int userId, string domain);
        Task<bool> ProcessPaymentSuccessAsync(string sessionId, int courseId, int userId);
    }
}
