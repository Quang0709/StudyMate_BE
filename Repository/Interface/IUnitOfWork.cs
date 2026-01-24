using System;
using System.Threading.Tasks;
using Repository.Model;

namespace Repository.Interface;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<User> Users { get; }
    IGenericRepository<Student> Students { get; }
    IGenericRepository<Teacher> Teachers { get; }
    IGenericRepository<RefreshToken> RefreshTokens { get; }
    IGenericRepository<OtpCode> OtpCodes { get; }
    IGenericRepository<EmailVerificationToken> EmailVerificationTokens { get; }
    IGenericRepository<Course> Courses { get; }
    IGenericRepository<Enrollment> Enrollments { get; }
    IGenericRepository<CourseRating> CourseRatings { get; }
    IGenericRepository<Post> Posts { get; }
    IGenericRepository<Comment> Comments { get; }
    IGenericRepository<Transaction> Transactions { get; }
    IGenericRepository<PremiumPlan> PremiumPlans { get; }
    IGenericRepository<UserSubscription> UserSubscriptions { get; }
    IGenericRepository<Flashcard> Flashcards { get; }
    IGenericRepository<Quiz> Quizzes { get; }
    IGenericRepository<PasswordResetCode> PasswordResetCodes { get; }
    
    Task<int> SaveChangesAsync();
}
