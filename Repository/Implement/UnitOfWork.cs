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
    private IGenericRepository<Category>? _categories;
    private IGenericRepository<RefreshToken>? _refreshTokens;
    private IGenericRepository<OtpCode>? _otpCodes;
    private IGenericRepository<EmailVerificationToken>? _emailVerificationTokens;
    private IGenericRepository<Course>? _courses;
    private IGenericRepository<Enrollment>? _enrollments;
    private IGenericRepository<CourseRating>? _courseRatings;
    private IGenericRepository<Post>? _posts;
    private IGenericRepository<Comment>? _comments;
    private IGenericRepository<PostLike>? _postLikes;
    private IGenericRepository<Feedback>? _feedbacks;
    private IGenericRepository<Transaction>? _transactions;
    private IGenericRepository<PremiumPlan>? _premiumPlans;
    private IGenericRepository<UserSubscription>? _userSubscriptions;
    private IGenericRepository<Flashcard>? _flashcards;
    private IGenericRepository<Quiz>? _quizzes;
    private IGenericRepository<QuizQuestion>? _quizQuestions;
    private IGenericRepository<QuizOption>? _quizOptions;
    private IGenericRepository<CourseSection>? _courseSections;
    private IGenericRepository<SectionMaterial>? _sectionMaterials;
    private IGenericRepository<PasswordResetCode>? _passwordResetCodes;
    private IGenericRepository<PaymentOrder>? _paymentOrders;

    public UnitOfWork(StudyMateContext context)
    {
        _context = context;
    }

    public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context);
    public IGenericRepository<Student> Students => _students ??= new GenericRepository<Student>(_context);
    public IGenericRepository<Teacher> Teachers => _teachers ??= new GenericRepository<Teacher>(_context);
    public IGenericRepository<Category> Categories => _categories ??= new GenericRepository<Category>(_context);
    public IGenericRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new GenericRepository<RefreshToken>(_context);
    public IGenericRepository<OtpCode> OtpCodes => _otpCodes ??= new GenericRepository<OtpCode>(_context);
    public IGenericRepository<EmailVerificationToken> EmailVerificationTokens => _emailVerificationTokens ??= new GenericRepository<EmailVerificationToken>(_context);
    public IGenericRepository<Course> Courses => _courses ??= new GenericRepository<Course>(_context);
    public IGenericRepository<Enrollment> Enrollments => _enrollments ??= new GenericRepository<Enrollment>(_context);
    public IGenericRepository<CourseRating> CourseRatings => _courseRatings ??= new GenericRepository<CourseRating>(_context);
    public IGenericRepository<Post> Posts => _posts ??= new GenericRepository<Post>(_context);
    public IGenericRepository<Comment> Comments => _comments ??= new GenericRepository<Comment>(_context);
    public IGenericRepository<PostLike> PostLikes => _postLikes ??= new GenericRepository<PostLike>(_context);
    public IGenericRepository<Feedback> Feedbacks => _feedbacks ??= new GenericRepository<Feedback>(_context);
    public IGenericRepository<Transaction> Transactions => _transactions ??= new GenericRepository<Transaction>(_context);
    public IGenericRepository<PremiumPlan> PremiumPlans => _premiumPlans ??= new GenericRepository<PremiumPlan>(_context);
    public IGenericRepository<UserSubscription> UserSubscriptions => _userSubscriptions ??= new GenericRepository<UserSubscription>(_context);
    public IGenericRepository<Flashcard> Flashcards => _flashcards ??= new GenericRepository<Flashcard>(_context);
    public IGenericRepository<Quiz> Quizzes => _quizzes ??= new GenericRepository<Quiz>(_context);
    public IGenericRepository<QuizQuestion> QuizQuestions => _quizQuestions ??= new GenericRepository<QuizQuestion>(_context);
    public IGenericRepository<QuizOption> QuizOptions => _quizOptions ??= new GenericRepository<QuizOption>(_context);
    public IGenericRepository<CourseSection> CourseSections => _courseSections ??= new GenericRepository<CourseSection>(_context);
    public IGenericRepository<SectionMaterial> SectionMaterials => _sectionMaterials ??= new GenericRepository<SectionMaterial>(_context);
    public IGenericRepository<PasswordResetCode> PasswordResetCodes => _passwordResetCodes ??= new GenericRepository<PasswordResetCode>(_context);
    public IGenericRepository<PaymentOrder> PaymentOrders => _paymentOrders ??= new GenericRepository<PaymentOrder>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
