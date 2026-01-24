using System;
using System.Threading.Tasks;
using Repository.Data;
using Repository.Interface;
using Repository.Model;

namespace Repository.Implement;

public class UnitOfWork : IUnitOfWork
{
    private readonly StudyMateContext _context;
    
    private IGenericRepository<User>? _users;
    private IGenericRepository<Student>? _students;
    private IGenericRepository<Teacher>? _teachers;
    private IGenericRepository<RefreshToken>? _refreshTokens;
    private IGenericRepository<OtpCode>? _otpCodes;
    private IGenericRepository<EmailVerificationToken>? _emailVerificationTokens;
    private IGenericRepository<Course>? _courses;
    private IGenericRepository<Enrollment>? _enrollments;
    private IGenericRepository<CourseRating>? _courseRatings;
    private IGenericRepository<Post>? _posts;
    private IGenericRepository<Comment>? _comments;
    private IGenericRepository<Transaction>? _transactions;
    private IGenericRepository<PremiumPlan>? _premiumPlans;
    private IGenericRepository<UserSubscription>? _userSubscriptions;
    private IGenericRepository<Flashcard>? _flashcards;
    private IGenericRepository<Quiz>? _quizzes;
    private IGenericRepository<PasswordResetCode>? _passwordResetCodes;

    public UnitOfWork(StudyMateContext context)
    {
        _context = context;
    }

    public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context);
    public IGenericRepository<Student> Students => _students ??= new GenericRepository<Student>(_context);
    public IGenericRepository<Teacher> Teachers => _teachers ??= new GenericRepository<Teacher>(_context);
    public IGenericRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new GenericRepository<RefreshToken>(_context);
    public IGenericRepository<OtpCode> OtpCodes => _otpCodes ??= new GenericRepository<OtpCode>(_context);
    public IGenericRepository<EmailVerificationToken> EmailVerificationTokens => _emailVerificationTokens ??= new GenericRepository<EmailVerificationToken>(_context);
    public IGenericRepository<Course> Courses => _courses ??= new GenericRepository<Course>(_context);
    public IGenericRepository<Enrollment> Enrollments => _enrollments ??= new GenericRepository<Enrollment>(_context);
    public IGenericRepository<CourseRating> CourseRatings => _courseRatings ??= new GenericRepository<CourseRating>(_context);
    public IGenericRepository<Post> Posts => _posts ??= new GenericRepository<Post>(_context);
    public IGenericRepository<Comment> Comments => _comments ??= new GenericRepository<Comment>(_context);
    public IGenericRepository<Transaction> Transactions => _transactions ??= new GenericRepository<Transaction>(_context);
    public IGenericRepository<PremiumPlan> PremiumPlans => _premiumPlans ??= new GenericRepository<PremiumPlan>(_context);
    public IGenericRepository<UserSubscription> UserSubscriptions => _userSubscriptions ??= new GenericRepository<UserSubscription>(_context);
    public IGenericRepository<Flashcard> Flashcards => _flashcards ??= new GenericRepository<Flashcard>(_context);
    public IGenericRepository<Quiz> Quizzes => _quizzes ??= new GenericRepository<Quiz>(_context);
    public IGenericRepository<PasswordResetCode> PasswordResetCodes => _passwordResetCodes ??= new GenericRepository<PasswordResetCode>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
