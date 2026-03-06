using Manhaj.Models;

namespace Manhaj.Services
{
    public interface IQuizService
    {
        Task<bool> SubmitQuizAsync(int quizId, int studentId, Dictionary<int, int> answers);
    }
}
