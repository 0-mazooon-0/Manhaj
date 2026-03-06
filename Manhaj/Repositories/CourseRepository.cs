using Manhaj.DTOs;
using Manhaj.IRepositories;
using Manhaj.Models;
using Microsoft.EntityFrameworkCore;

namespace Manhaj.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        ManhajDbContext _DB;

        public CourseRepository(ManhajDbContext dB)
        {
            _DB = dB;
        }



        public void Create(CourseVM courseDto)
        {
            var course = new Course
            {
                Title = courseDto.Title,
                Description = courseDto.Description,
                Level = courseDto.Level,
                Price = courseDto.Price,
                Duration = courseDto.Duration,
                NumberOfLectures = courseDto.NumberOfLectures,
                TeacherId = courseDto.TeacherId,
                CreationDate = DateTime.UtcNow
            };
            _DB.Courses.Add(course);
            _DB.SaveChanges();
        }

        public bool Delete(int id)
        {
            Course Course = _DB.Courses
                .Include(c => c.Course_Registrations)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Materials)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Assignment)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Quiz)
                        .ThenInclude(q => q.Questions)
                            .ThenInclude(qu => qu.Options)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Quiz)
                        .ThenInclude(q => q.Quizzes)
                .AsSplitQuery()
                .FirstOrDefault(c => c.Id == id);
                
            if (Course == null)
            {
                return false;
            }

            // Delete all related entities
            foreach (var lecture in Course.Lectures.ToList())
            {
                // Delete student lecture progress
                var studentLectures = _DB.Student_Lectures.Where(sl => sl.LectureId == lecture.Id);
                _DB.Student_Lectures.RemoveRange(studentLectures);

                // Delete quiz and its questions/options
                if (lecture.Quiz != null)
                {
                    foreach (var question in lecture.Quiz.Questions.ToList())
                    {
                        _DB.Options.RemoveRange(question.Options);
                        _DB.Questions.Remove(question);
                    }
                    _DB.Student_Quizzes.RemoveRange(lecture.Quiz.Quizzes);
                    _DB.Quizzes.Remove(lecture.Quiz);
                }

                // Delete assignment
                if (lecture.Assignment != null)
                {
                    var studentAssignments = _DB.Student_Assignments.Where(sa => sa.AssignmentId == lecture.Assignment.Id);
                    _DB.Student_Assignments.RemoveRange(studentAssignments);
                    _DB.Assignments.Remove(lecture.Assignment);
                }

                // Delete materials
                _DB.Materials.RemoveRange(lecture.Materials);
                
                // Delete lecture
                _DB.Lectures.Remove(lecture);
            }

            // Delete course registrations
            _DB.Course_Registrations.RemoveRange(Course.Course_Registrations);

            // Delete the course itself
            _DB.Courses.Remove(Course);
            _DB.SaveChanges();
            return true;
        }

        public List<CourseVM> GetAll()
        {
            var courses = _DB.Courses
                 .Select(c => new CourseVM
                 {
                     Id = c.Id,
                     Title = c.Title,
                     Description = c.Description,
                     Level = c.Level,
                     Price = c.Price,
                     Duration = c.Duration,
                     NumberOfLectures = c.NumberOfLectures,
                     CreationDate = c.CreationDate,
                     TeacherId = c.TeacherId,
                 })
                 .ToList();
            return courses;
        }

        public CourseVM GetById(int id)
        {
            var course = _DB.Courses.FirstOrDefault(c => c.Id == id);

            if (course is null)
            {
                return null;
            }

            return new CourseVM
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Level = course.Level,
                Price = course.Price,
                Duration = course.Duration,
                NumberOfLectures = course.NumberOfLectures,
                CreationDate = course.CreationDate,
                TeacherId = course.TeacherId
            };
        }



        public CourseVM GetByName(string name)
        {
            var course = _DB.Courses.FirstOrDefault(c => c.Title == name);

            if (course is null)
            {
                return null;
            }

            return new CourseVM
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Level = course.Level,
                Price = course.Price,
                Duration = course.Duration,
                NumberOfLectures = course.NumberOfLectures,
                CreationDate = course.CreationDate,
                TeacherId = course.TeacherId
            };
        }

        public bool Update(CourseVM courseDto, int id)
        {
            if (courseDto == null)
                return false;

            Course oldCourse = _DB.Courses.FirstOrDefault(c => c.Id == id);

            if (oldCourse is null)
                return false;

            oldCourse.Title = courseDto.Title;
            oldCourse.Description = courseDto.Description;
            oldCourse.Price = courseDto.Price;
            oldCourse.NumberOfLectures = courseDto.NumberOfLectures;
            oldCourse.Level = courseDto.Level;
            oldCourse.Duration = courseDto.Duration;
            oldCourse.TeacherId = courseDto.TeacherId;


            _DB.SaveChanges();
            return true;
        }

        public Course GetCourseWithDetails(int id)
        {
            return _DB.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Course_Registrations)
                    .ThenInclude(cr => cr.Student)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Materials)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Assignment)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Quiz)
                .AsSplitQuery()
                .FirstOrDefault(c => c.Id == id);
        }

        public List<Course> GetCoursesByTeacher(int teacherId)
        {
            return _DB.Courses
                .Where(c => c.TeacherId == teacherId)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Assignment)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Quiz)
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.Materials)
                .Include(c => c.Course_Registrations)
                .AsSplitQuery()
                .ToList();
        }

        public List<Course> GetAllCoursesWithDetails()
        {
            return _DB.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lectures)
                .Include(c => c.Course_Registrations)
                .ToList();
        }
    }
}
