using Repository.Interface;
using Repository.Model;
using Service.DTOs;
using Service.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Implement;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool Success, string Message, object? Data)> CreateCourseAsync(int teacherId, CreateCourseDto createCourseDto)
    {
        try
        {
            // Validate teacher exists
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(teacherId);
            if (teacher == null)
            {
                return (false, "Teacher not found", null);
            }

            // Validate quiz limits
            if (createCourseDto.Quizzes != null)
            {
                foreach (var quiz in createCourseDto.Quizzes)
                {
                    if (quiz.Questions.Count > 50)
                    {
                        return (false, $"Quiz '{quiz.Title}' has more than 50 questions", null);
                    }

                    // Validate each question has at least one correct answer
                    foreach (var question in quiz.Questions)
                    {
                        if (!question.Options.Any(o => o.IsCorrect))
                        {
                            return (false, $"Question '{question.QuestionText}' must have at least one correct answer", null);
                        }
                    }
                }
            }

            // Validate flashcard limits
            if (createCourseDto.Flashcards != null && createCourseDto.Flashcards.Count > 50)
            {
                return (false, "Course cannot have more than 50 flashcards", null);
            }

            // Create course
            var course = new Course
            {
                TeacherId = teacherId,
                Title = createCourseDto.Title,
                Description = createCourseDto.Description,
                Price = createCourseDto.Price ?? 0,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.SaveChangesAsync();

            // Create sections with materials
            if (createCourseDto.Sections != null && createCourseDto.Sections.Any())
            {
                foreach (var sectionDto in createCourseDto.Sections)
                {
                    var section = new CourseSection
                    {
                        CourseId = course.Id,
                        Title = sectionDto.Title,
                        Description = sectionDto.Description,
                        OrderIndex = sectionDto.OrderIndex,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.CourseSections.AddAsync(section);
                    await _unitOfWork.SaveChangesAsync();

                    // Add materials to section
                    if (sectionDto.Materials != null && sectionDto.Materials.Any())
                    {
                        foreach (var materialDto in sectionDto.Materials)
                        {
                            var material = new SectionMaterial
                            {
                                SectionId = section.Id,
                                Title = materialDto.Title,
                                Description = materialDto.Description,
                                MaterialType = materialDto.MaterialType,
                                FileUrl = materialDto.FileUrl,
                                FileName = materialDto.FileName,
                                FileSize = materialDto.FileSize,
                                OrderIndex = materialDto.OrderIndex,
                                CreatedAt = DateTime.UtcNow
                            };

                            await _unitOfWork.SectionMaterials.AddAsync(material);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }

            // Create quizzes
            if (createCourseDto.Quizzes != null && createCourseDto.Quizzes.Any())
            {
                foreach (var quizDto in createCourseDto.Quizzes)
                {
                    var quiz = new Quiz
                    {
                        CourseId = course.Id,
                        Title = quizDto.Title
                    };

                    await _unitOfWork.Quizzes.AddAsync(quiz);
                    await _unitOfWork.SaveChangesAsync();

                    foreach (var questionDto in quizDto.Questions)
                    {
                        var question = new QuizQuestion
                        {
                            QuizId = quiz.Id,
                            QuestionText = questionDto.QuestionText,
                            QuestionType = questionDto.QuestionType,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.QuizQuestions.AddAsync(question);
                        await _unitOfWork.SaveChangesAsync();

                        foreach (var optionDto in questionDto.Options)
                        {
                            var option = new QuizOption
                            {
                                QuestionId = question.Id,
                                OptionText = optionDto.OptionText,
                                IsCorrect = optionDto.IsCorrect
                            };

                            await _unitOfWork.QuizOptions.AddAsync(option);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }

            // Create flashcards
            if (createCourseDto.Flashcards != null && createCourseDto.Flashcards.Any())
            {
                foreach (var flashcardDto in createCourseDto.Flashcards)
                {
                    var flashcard = new Flashcard
                    {
                        CourseId = course.Id,
                        Front = flashcardDto.Front,
                        Back = flashcardDto.Back
                    };

                    await _unitOfWork.Flashcards.AddAsync(flashcard);
                }

                await _unitOfWork.SaveChangesAsync();
            }

            return (true, "Course created successfully", new
            {
                CourseId = course.Id,
                Title = course.Title,
                Status = course.Status,
                SectionCount = createCourseDto.Sections?.Count ?? 0,
                QuizCount = createCourseDto.Quizzes?.Count ?? 0,
                FlashcardCount = createCourseDto.Flashcards?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            return (false, $"Error creating course: {ex.Message}", null);
        }
    }
}
