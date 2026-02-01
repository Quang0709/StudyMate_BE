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

    public async Task<(bool Success, string Message, object? Data)> UpdateCourseAsync(int courseId, int userId, UpdateCourseDto updateCourseDto)
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
                return (false, "You don't have permission to edit this course", null);
            }

            // Check if course is deleted
            if (course.IsDeleted)
            {
                return (false, "Cannot edit a deleted course", null);
            }

            // Validate category if provided
            if (updateCourseDto.CategoryId.HasValue)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(updateCourseDto.CategoryId.Value);
                if (category == null)
                {
                    return (false, $"Category with ID {updateCourseDto.CategoryId.Value} not found", null);
                }
            }

            // Validate quiz limits
            if (updateCourseDto.Quizzes != null)
            {
                foreach (var quiz in updateCourseDto.Quizzes)
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
            if (updateCourseDto.Flashcards != null && updateCourseDto.Flashcards.Count > 50)
            {
                return (false, "Course cannot have more than 50 flashcards", null);
            }

            // Update course basic info
            course.Title = updateCourseDto.Title;
            course.Description = updateCourseDto.Description;
            course.Price = updateCourseDto.Price ?? 0;
            course.CategoryId = updateCourseDto.CategoryId;
            course.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Courses.Update(course);
            await _unitOfWork.SaveChangesAsync();

            // Update sections
            await UpdateCourseSectionsAsync(course.Id, updateCourseDto.Sections);

            // Update quizzes
            await UpdateCourseQuizzesAsync(course.Id, updateCourseDto.Quizzes);

            // Update flashcards
            await UpdateCourseFlashcardsAsync(course.Id, updateCourseDto.Flashcards);

            await _unitOfWork.SaveChangesAsync();

            return (true, "Course updated successfully", new
            {
                CourseId = course.Id,
                Title = course.Title,
                Status = course.Status,
                SectionCount = updateCourseDto.Sections?.Count ?? 0,
                QuizCount = updateCourseDto.Quizzes?.Count ?? 0,
                FlashcardCount = updateCourseDto.Flashcards?.Count ?? 0
            });
        }
        catch (Exception ex)
        {
            return (false, $"Error updating course: {ex.Message}", null);
        }
    }

    private async Task UpdateCourseSectionsAsync(int courseId, List<UpdateCourseSectionDto>? sectionDtos)
    {
        // Get existing sections
        var existingSections = await _unitOfWork.CourseSections.GetAllAsync();
        var courseSections = existingSections.Where(s => s.CourseId == courseId).ToList();

        if (sectionDtos == null || !sectionDtos.Any())
        {
            // Delete all sections if none provided
            foreach (var section in courseSections)
            {
                _unitOfWork.CourseSections.Delete(section);
            }
            return;
        }

        var sectionIdsToKeep = sectionDtos.Where(s => s.Id.HasValue).Select(s => s.Id.Value).ToList();

        // Delete sections that are not in the update list
        foreach (var section in courseSections)
        {
            if (!sectionIdsToKeep.Contains(section.Id))
            {
                _unitOfWork.CourseSections.Delete(section);
            }
        }

        // Update or create sections
        foreach (var sectionDto in sectionDtos)
        {
            if (sectionDto.Id.HasValue)
            {
                // Update existing section
                var section = courseSections.FirstOrDefault(s => s.Id == sectionDto.Id.Value);
                if (section != null)
                {
                    section.Title = sectionDto.Title;
                    section.Description = sectionDto.Description;
                    section.OrderIndex = sectionDto.OrderIndex;
                    _unitOfWork.CourseSections.Update(section);
                    await _unitOfWork.SaveChangesAsync();

                    // Update materials
                    await UpdateSectionMaterialsAsync(section.Id, sectionDto.Materials);
                }
            }
            else
            {
                // Create new section
                var newSection = new CourseSection
                {
                    CourseId = courseId,
                    Title = sectionDto.Title,
                    Description = sectionDto.Description,
                    OrderIndex = sectionDto.OrderIndex,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.CourseSections.AddAsync(newSection);
                await _unitOfWork.SaveChangesAsync();

                // Add materials
                if (sectionDto.Materials != null && sectionDto.Materials.Any())
                {
                    foreach (var materialDto in sectionDto.Materials)
                    {
                        var material = new SectionMaterial
                        {
                            SectionId = newSection.Id,
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
        }
    }

    private async Task UpdateSectionMaterialsAsync(int sectionId, List<UpdateSectionMaterialDto>? materialDtos)
    {
        // Get existing materials
        var existingMaterials = await _unitOfWork.SectionMaterials.GetAllAsync();
        var sectionMaterials = existingMaterials.Where(m => m.SectionId == sectionId).ToList();

        if (materialDtos == null || !materialDtos.Any())
        {
            // Delete all materials if none provided
            foreach (var material in sectionMaterials)
            {
                _unitOfWork.SectionMaterials.Delete(material);
            }
            return;
        }

        var materialIdsToKeep = materialDtos.Where(m => m.Id.HasValue).Select(m => m.Id.Value).ToList();

        // Delete materials that are not in the update list
        foreach (var material in sectionMaterials)
        {
            if (!materialIdsToKeep.Contains(material.Id))
            {
                _unitOfWork.SectionMaterials.Delete(material);
            }
        }

        // Update or create materials
        foreach (var materialDto in materialDtos)
        {
            if (materialDto.Id.HasValue)
            {
                // Update existing material
                var material = sectionMaterials.FirstOrDefault(m => m.Id == materialDto.Id.Value);
                if (material != null)
                {
                    material.Title = materialDto.Title;
                    material.Description = materialDto.Description;
                    material.MaterialType = materialDto.MaterialType;
                    material.FileUrl = materialDto.FileUrl;
                    material.FileName = materialDto.FileName;
                    material.FileSize = materialDto.FileSize;
                    material.OrderIndex = materialDto.OrderIndex;
                    _unitOfWork.SectionMaterials.Update(material);
                }
            }
            else
            {
                // Create new material
                var newMaterial = new SectionMaterial
                {
                    SectionId = sectionId,
                    Title = materialDto.Title,
                    Description = materialDto.Description,
                    MaterialType = materialDto.MaterialType,
                    FileUrl = materialDto.FileUrl,
                    FileName = materialDto.FileName,
                    FileSize = materialDto.FileSize,
                    OrderIndex = materialDto.OrderIndex,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.SectionMaterials.AddAsync(newMaterial);
            }
        }
    }

    private async Task UpdateCourseQuizzesAsync(int courseId, List<UpdateQuizDto>? quizDtos)
    {
        // Get existing quizzes
        var existingQuizzes = await _unitOfWork.Quizzes.GetAllAsync();
        var courseQuizzes = existingQuizzes.Where(q => q.CourseId == courseId).ToList();

        if (quizDtos == null || !quizDtos.Any())
        {
            // Delete all quizzes if none provided
            foreach (var quiz in courseQuizzes)
            {
                _unitOfWork.Quizzes.Delete(quiz);
            }
            return;
        }

        var quizIdsToKeep = quizDtos.Where(q => q.Id.HasValue).Select(q => q.Id.Value).ToList();

        // Delete quizzes that are not in the update list
        foreach (var quiz in courseQuizzes)
        {
            if (!quizIdsToKeep.Contains(quiz.Id))
            {
                _unitOfWork.Quizzes.Delete(quiz);
            }
        }

        // Update or create quizzes
        foreach (var quizDto in quizDtos)
        {
            if (quizDto.Id.HasValue)
            {
                // Update existing quiz
                var quiz = courseQuizzes.FirstOrDefault(q => q.Id == quizDto.Id.Value);
                if (quiz != null)
                {
                    quiz.Title = quizDto.Title;
                    _unitOfWork.Quizzes.Update(quiz);
                    await _unitOfWork.SaveChangesAsync();

                    // Update questions
                    await UpdateQuizQuestionsAsync(quiz.Id, quizDto.Questions);
                }
            }
            else
            {
                // Create new quiz
                var newQuiz = new Quiz
                {
                    CourseId = courseId,
                    Title = quizDto.Title
                };
                await _unitOfWork.Quizzes.AddAsync(newQuiz);
                await _unitOfWork.SaveChangesAsync();

                // Add questions
                foreach (var questionDto in quizDto.Questions)
                {
                    var question = new QuizQuestion
                    {
                        QuizId = newQuiz.Id,
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
        }
    }

    private async Task UpdateQuizQuestionsAsync(int quizId, List<UpdateQuizQuestionDto> questionDtos)
    {
        // Get existing questions
        var existingQuestions = await _unitOfWork.QuizQuestions.GetAllAsync();
        var quizQuestions = existingQuestions.Where(q => q.QuizId == quizId).ToList();

        var questionIdsToKeep = questionDtos.Where(q => q.Id.HasValue).Select(q => q.Id.Value).ToList();

        // Delete questions that are not in the update list
        foreach (var question in quizQuestions)
        {
            if (!questionIdsToKeep.Contains(question.Id))
            {
                _unitOfWork.QuizQuestions.Delete(question);
            }
        }

        // Update or create questions
        foreach (var questionDto in questionDtos)
        {
            if (questionDto.Id.HasValue)
            {
                // Update existing question
                var question = quizQuestions.FirstOrDefault(q => q.Id == questionDto.Id.Value);
                if (question != null)
                {
                    question.QuestionText = questionDto.QuestionText;
                    question.QuestionType = questionDto.QuestionType;
                    _unitOfWork.QuizQuestions.Update(question);
                    await _unitOfWork.SaveChangesAsync();

                    // Update options
                    await UpdateQuizOptionsAsync(question.Id, questionDto.Options);
                }
            }
            else
            {
                // Create new question
                var newQuestion = new QuizQuestion
                {
                    QuizId = quizId,
                    QuestionText = questionDto.QuestionText,
                    QuestionType = questionDto.QuestionType,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.QuizQuestions.AddAsync(newQuestion);
                await _unitOfWork.SaveChangesAsync();

                // Add options
                foreach (var optionDto in questionDto.Options)
                {
                    var option = new QuizOption
                    {
                        QuestionId = newQuestion.Id,
                        OptionText = optionDto.OptionText,
                        IsCorrect = optionDto.IsCorrect
                    };
                    await _unitOfWork.QuizOptions.AddAsync(option);
                }
            }
        }
    }

    private async Task UpdateQuizOptionsAsync(int questionId, List<UpdateQuizOptionDto> optionDtos)
    {
        // Get existing options
        var existingOptions = await _unitOfWork.QuizOptions.GetAllAsync();
        var questionOptions = existingOptions.Where(o => o.QuestionId == questionId).ToList();

        var optionIdsToKeep = optionDtos.Where(o => o.Id.HasValue).Select(o => o.Id.Value).ToList();

        // Delete options that are not in the update list
        foreach (var option in questionOptions)
        {
            if (!optionIdsToKeep.Contains(option.Id))
            {
                _unitOfWork.QuizOptions.Delete(option);
            }
        }

        // Update or create options
        foreach (var optionDto in optionDtos)
        {
            if (optionDto.Id.HasValue)
            {
                // Update existing option
                var option = questionOptions.FirstOrDefault(o => o.Id == optionDto.Id.Value);
                if (option != null)
                {
                    option.OptionText = optionDto.OptionText;
                    option.IsCorrect = optionDto.IsCorrect;
                    _unitOfWork.QuizOptions.Update(option);
                }
            }
            else
            {
                // Create new option
                var newOption = new QuizOption
                {
                    QuestionId = questionId,
                    OptionText = optionDto.OptionText,
                    IsCorrect = optionDto.IsCorrect
                };
                await _unitOfWork.QuizOptions.AddAsync(newOption);
            }
        }
    }

    private async Task UpdateCourseFlashcardsAsync(int courseId, List<UpdateFlashcardDto>? flashcardDtos)
    {
        // Get existing flashcards
        var existingFlashcards = await _unitOfWork.Flashcards.GetAllAsync();
        var courseFlashcards = existingFlashcards.Where(f => f.CourseId == courseId).ToList();

        if (flashcardDtos == null || !flashcardDtos.Any())
        {
            // Delete all flashcards if none provided
            foreach (var flashcard in courseFlashcards)
            {
                _unitOfWork.Flashcards.Delete(flashcard);
            }
            return;
        }

        var flashcardIdsToKeep = flashcardDtos.Where(f => f.Id.HasValue).Select(f => f.Id.Value).ToList();

        // Delete flashcards that are not in the update list
        foreach (var flashcard in courseFlashcards)
        {
            if (!flashcardIdsToKeep.Contains(flashcard.Id))
            {
                _unitOfWork.Flashcards.Delete(flashcard);
            }
        }

        // Update or create flashcards
        foreach (var flashcardDto in flashcardDtos)
        {
            if (flashcardDto.Id.HasValue)
            {
                // Update existing flashcard
                var flashcard = courseFlashcards.FirstOrDefault(f => f.Id == flashcardDto.Id.Value);
                if (flashcard != null)
                {
                    flashcard.Front = flashcardDto.Front;
                    flashcard.Back = flashcardDto.Back;
                    _unitOfWork.Flashcards.Update(flashcard);
                }
            }
            else
            {
                // Create new flashcard
                var newFlashcard = new Flashcard
                {
                    CourseId = courseId,
                    Front = flashcardDto.Front,
                    Back = flashcardDto.Back
                };
                await _unitOfWork.Flashcards.AddAsync(newFlashcard);
            }
        }
    }
}
