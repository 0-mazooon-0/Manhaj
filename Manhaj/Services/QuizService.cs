using Manhaj.Models;
using Microsoft.EntityFrameworkCore;

namespace Manhaj.Services
{
    public class QuizService : IQuizService
    {
        private readonly ManhajDbContext _context;

        public QuizService(ManhajDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SubmitQuizAsync(int quizId, int studentId, Dictionary<int, int> answers)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qu => qu.Options)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return false;

            var attempt = await _context.Student_Quizzes
                .FirstOrDefaultAsync(sq => sq.QuizId == quizId && sq.StudentId == studentId);

            if (attempt == null || attempt.IsSubmitted) return false;

            decimal score = 0;
            if (answers != null)
            {
                foreach (var question in quiz.Questions)
                {
                    if (answers.ContainsKey(question.Id))
                    {
                        var selectedOptionId = answers[question.Id];
                        var selectedOption = question.Options.FirstOrDefault(o => o.Id == selectedOptionId);

                        if (selectedOption != null)
                        {
                            var answerRecord = new Student_Quiz_Answer
                            {
                                StudentId = studentId,
                                QuizId = quizId,
                                QuestionId = question.Id,
                                SelectedOptionId = selectedOptionId
                            };
                            _context.Student_Quiz_Answers.Add(answerRecord);

                            if (selectedOption.Content == question.TrueAnswer)
                            {
                                score += question.Points;
                            }
                        }
                    }
                }
            }

            attempt.Grade = score;
            attempt.SubmissionDate = DateTime.Now;
            attempt.IsSubmitted = true;

            _context.Student_Quizzes.Update(attempt);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
