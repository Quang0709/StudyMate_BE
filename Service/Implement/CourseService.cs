using Repository.Interface;
using Repository.Model;
using Service.DTOs;
using Service.Interface;
using System;
using System.Collections.Generic;
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

    public async Task<(bool Success, string Message, object? Data)> CreateCourseAsync(int userId, CreateCourseDto createCourseDto)
    {
        try
        {
            // Get teacher record from user ID
            var teachers = await _unitOfWork.Teachers.GetAllAsync();
            var teacher = teachers.FirstOrDefault(t => t.UserId == userId);
            
            if (teacher == null)
            {
                return (false, "Teacher profile not found for this user", null);
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

            // Validate category if provided
            if (createCourseDto.CategoryId.HasValue)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(createCourseDto.CategoryId.Value);
                if (category == null)
                {
                    return (false, $"Category with ID {createCourseDto.CategoryId.Value} not found", null);
                }
            }

            // Create course
            var course = new Course
            {
                TeacherId = teacher.Id, // Use teacher.Id instead of userId
                Title = createCourseDto.Title,
                Description = createCourseDto.Description,
                Price = createCourseDto.Price ?? 0,
                CategoryId = createCourseDto.CategoryId,
                Status = "pending",
                IsDeleted = false,
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

    public async Task<(bool Success, string Message, List<CourseResponseDto>? Data)> GetMyCoursesAsync(int userId)
    {
        try
        {
            // Get teacher record from user ID
            var teachers = await _unitOfWork.Teachers.GetAllAsync();
            var teacher = teachers.FirstOrDefault(t => t.UserId == userId);
            
            if (teacher == null)
            {
                return (false, "Teacher profile not found for this user", null);
            }

            // Get all courses by teacher (ALWAYS exclude soft-deleted courses)
            var allCourses = await _unitOfWork.Courses.GetAllAsync();
            var teacherCourses = allCourses
                .Where(c => c.TeacherId == teacher.Id && !c.IsDeleted)
                .ToList();

            // Get all related data
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var sections = await _unitOfWork.CourseSections.GetAllAsync();
            var materials = await _unitOfWork.SectionMaterials.GetAllAsync();
            var quizzes = await _unitOfWork.Quizzes.GetAllAsync();
            var questions = await _unitOfWork.QuizQuestions.GetAllAsync();
            var options = await _unitOfWork.QuizOptions.GetAllAsync();
            var flashcards = await _unitOfWork.Flashcards.GetAllAsync();
            var enrollments = await _unitOfWork.Enrollments.GetAllAsync();

            var courseList = new List<CourseResponseDto>();

            foreach (var course in teacherCourses.OrderByDescending(c => c.CreatedAt))
            {
                var category = categories.FirstOrDefault(cat => cat.Id == course.CategoryId);
                var courseSections = sections.Where(s => s.CourseId == course.Id).OrderBy(s => s.OrderIndex).ToList();
                var courseQuizzes = quizzes.Where(q => q.CourseId == course.Id).ToList();
                var courseFlashcards = flashcards.Where(f => f.CourseId == course.Id).ToList();
                var courseEnrollments = enrollments.Where(e => e.CourseId == course.Id).ToList();

                var sectionDtos = new List<CourseSectionDto>();
                var totalMaterials = 0;

                foreach (var section in courseSections)
                {
                    var sectionMaterials = materials.Where(m => m.SectionId == section.Id).OrderBy(m => m.OrderIndex).ToList();
                    totalMaterials += sectionMaterials.Count;

                    sectionDtos.Add(new CourseSectionDto
                    {
                        Id = section.Id,
                        Title = section.Title,
                        Description = section.Description,
                        OrderIndex = section.OrderIndex,
                        CreatedAt = section.CreatedAt,
                        Materials = sectionMaterials.Select(m => new SectionMaterialDto
                        {
                            Id = m.Id,
                            Title = m.Title,
                            Description = m.Description,
                            MaterialType = m.MaterialType,
                            FileUrl = m.FileUrl,
                            FileName = m.FileName,
                            FileSize = m.FileSize,
                            OrderIndex = m.OrderIndex
                        }).ToList()
                    });
                }

                var quizDtos = new List<QuizDto>();
                var totalQuestions = 0;

                foreach (var quiz in courseQuizzes)
                {
                    var quizQuestions = questions.Where(q => q.QuizId == quiz.Id).ToList();
                    totalQuestions += quizQuestions.Count;

                    quizDtos.Add(new QuizDto
                    {
                        Id = quiz.Id,
                        Title = quiz.Title,
                        Questions = quizQuestions.Select(q => new QuizQuestionDto
                        {
                            Id = q.Id,
                            QuestionText = q.QuestionText,
                            QuestionType = q.QuestionType,
                            Options = options.Where(o => o.QuestionId == q.Id).Select(o => new QuizOptionDto
                            {
                                Id = o.Id,
                                OptionText = o.OptionText,
                                IsCorrect = o.IsCorrect
                            }).ToList()
                        }).ToList()
                    });
                }

                var flashcardDtos = courseFlashcards.Select(f => new FlashcardDto
                {
                    Id = f.Id,
                    Front = f.Front,
                    Back = f.Back
                }).ToList();

                courseList.Add(new CourseResponseDto
                {
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    Price = course.Price ?? 0,
                    Status = course.Status ?? "pending",
                    RejectReason = course.RejectReason,
                    IsDeleted = course.IsDeleted,
                    CategoryId = course.CategoryId,
                    CategoryName = category?.Name,
                    CreatedAt = course.CreatedAt,
                    UpdatedAt = course.UpdatedAt,
                    DeletedAt = course.DeletedAt,
                    TotalSections = courseSections.Count,
                    TotalMaterials = totalMaterials,
                    TotalQuizzes = courseQuizzes.Count,
                    TotalQuestions = totalQuestions,
                    TotalFlashcards = courseFlashcards.Count,
                    TotalEnrollments = courseEnrollments.Count,
                    Sections = sectionDtos,
                    Quizzes = quizDtos,
                    Flashcards = flashcardDtos
                });
            }

            return (true, $"Retrieved {courseList.Count} active course(s) successfully", courseList);
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving courses: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message, CourseResponseDto? Data)> GetCourseDetailAsync(int courseId, int userId)
    {
        try
        {
            // Get teacher record from user ID
            var teachers = await _unitOfWork.Teachers.GetAllAsync();
            var teacher = teachers.FirstOrDefault(t => t.UserId == userId);
            
            if (teacher == null)
            {
                return (false, "Teacher profile not found for this user", null);
            }

            // Get course
            var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            
            if (course == null)
            {
                return (false, "Course not found", null);
            }

            // Verify ownership
            if (course.TeacherId != teacher.Id)
            {
                return (false, "You don't have permission to view this course", null);
            }

            // Get all related data
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var category = categories.FirstOrDefault(cat => cat.Id == course.CategoryId);
            
            var sections = await _unitOfWork.CourseSections.GetAllAsync();
            var courseSections = sections.Where(s => s.CourseId == course.Id).OrderBy(s => s.OrderIndex).ToList();
            
            var materials = await _unitOfWork.SectionMaterials.GetAllAsync();
            var quizzes = await _unitOfWork.Quizzes.GetAllAsync();
            var courseQuizzes = quizzes.Where(q => q.CourseId == course.Id).ToList();
            
            var questions = await _unitOfWork.QuizQuestions.GetAllAsync();
            var options = await _unitOfWork.QuizOptions.GetAllAsync();
            
            var flashcards = await _unitOfWork.Flashcards.GetAllAsync();
            var courseFlashcards = flashcards.Where(f => f.CourseId == course.Id).ToList();
            
            var enrollments = await _unitOfWork.Enrollments.GetAllAsync();
            var courseEnrollments = enrollments.Where(e => e.CourseId == course.Id).ToList();

            var sectionDtos = new List<CourseSectionDto>();
            var totalMaterials = 0;

            foreach (var section in courseSections)
            {
                var sectionMaterials = materials.Where(m => m.SectionId == section.Id).OrderBy(m => m.OrderIndex).ToList();
                totalMaterials += sectionMaterials.Count;

                sectionDtos.Add(new CourseSectionDto
                {
                    Id = section.Id,
                    Title = section.Title,
                    Description = section.Description,
                    OrderIndex = section.OrderIndex,
                    CreatedAt = section.CreatedAt,
                    Materials = sectionMaterials.Select(m => new SectionMaterialDto
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Description = m.Description,
                        MaterialType = m.MaterialType,
                        FileUrl = m.FileUrl,
                        FileName = m.FileName,
                        FileSize = m.FileSize,
                        OrderIndex = m.OrderIndex
                    }).ToList()
                });
            }

            var quizDtos = new List<QuizDto>();
            var totalQuestions = 0;

            foreach (var quiz in courseQuizzes)
            {
                var quizQuestions = questions.Where(q => q.QuizId == quiz.Id).ToList();
                totalQuestions += quizQuestions.Count;

                quizDtos.Add(new QuizDto
                {
                    Id = quiz.Id,
                    Title = quiz.Title,
                    Questions = quizQuestions.Select(q => new QuizQuestionDto
                    {
                        Id = q.Id,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType,
                        Options = options.Where(o => o.QuestionId == q.Id).Select(o => new QuizOptionDto
                        {
                            Id = o.Id,
                            OptionText = o.OptionText,
                            IsCorrect = o.IsCorrect
                        }).ToList()
                    }).ToList()
                });
            }

            var flashcardDtos = courseFlashcards.Select(f => new FlashcardDto
            {
                Id = f.Id,
                Front = f.Front,
                Back = f.Back
            }).ToList();

            var courseResponse = new CourseResponseDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price ?? 0,
                Status = course.Status ?? "pending",
                RejectReason = course.RejectReason,
                IsDeleted = course.IsDeleted,
                CategoryId = course.CategoryId,
                CategoryName = category?.Name,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                DeletedAt = course.DeletedAt,
                TotalSections = courseSections.Count,
                TotalMaterials = totalMaterials,
                TotalQuizzes = courseQuizzes.Count,
                TotalQuestions = totalQuestions,
                TotalFlashcards = courseFlashcards.Count,
                TotalEnrollments = courseEnrollments.Count,
                Sections = sectionDtos,
                Quizzes = quizDtos,
                Flashcards = flashcardDtos
            };

            return (true, "Course retrieved successfully", courseResponse);
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving course: {ex.Message}", null);
        }
    }
}
