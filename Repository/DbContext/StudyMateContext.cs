using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Repository.Model;

namespace Repository.Data;

public partial class StudyMateContext : DbContext
{
    public StudyMateContext()
    {
    }

    public StudyMateContext(DbContextOptions<StudyMateContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CourseRating> CourseRatings { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Flashcard> Flashcards { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PremiumPlan> PremiumPlans { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

    public virtual DbSet<QuizOption> QuizOptions { get; set; }

    public virtual DbSet<CourseSection> CourseSections { get; set; }

    public virtual DbSet<SectionMaterial> SectionMaterials { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserSubscription> UserSubscriptions { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<OtpCode> OtpCodes { get; set; }

    public virtual DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public virtual DbSet<PasswordResetCode> PasswordResetCodes { get; set; }
    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__comments__3213E83F3E22E7D1");

            entity.ToTable("comments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PostId).HasColumnName("post_id");

            entity.HasOne(d => d.Author).WithMany(p => p.Comments)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comment_User");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("FK_Comment_Post");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__categories");

            entity.ToTable("categories");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__courses__3213E83FB605C482");

            entity.ToTable("courses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.RejectReason).HasColumnName("reject_reason");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Courses)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Course_Teacher");

            entity.HasOne(d => d.Category).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Course_Category");
        });

        modelBuilder.Entity<CourseRating>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_r__3213E83FF292E4A6");

            entity.ToTable("course_ratings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseRatings)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rate_Course");

            entity.HasOne(d => d.Student).WithMany(p => p.CourseRatings)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Rate_Student");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__enrollme__3213E83FD1ADAE1D");

            entity.ToTable("enrollments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.EnrolledAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("enrolled_at");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enroll_Course");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enroll_Student");
        });

        modelBuilder.Entity<Flashcard>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__flashcar__3213E83FF5A50661");

            entity.ToTable("flashcards");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Front).HasColumnName("front");
            entity.Property(e => e.Back).HasColumnName("back");
            entity.Property(e => e.CourseId).HasColumnName("course_id");

            entity.HasOne(d => d.Course).WithMany(p => p.Flashcards)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_Flashcard_Course");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__posts__3213E83F0E6F997F");

            entity.ToTable("posts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Author).WithMany(p => p.Posts)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Post_User");
        });

        modelBuilder.Entity<PremiumPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__premium___3213E83F411D9987");

            entity.ToTable("premium_plans");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quizzes__3213E83F63D65F3A");

            entity.ToTable("quizzes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Course).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_Quiz_Course");
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_questions");

            entity.ToTable("quiz_questions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.QuizId).HasColumnName("quiz_id");
            entity.Property(e => e.QuestionText).HasColumnName("question_text");
            entity.Property(e => e.QuestionType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("question_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizQuestions)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_QuizQuestion_Quiz");
        });

        modelBuilder.Entity<QuizOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__quiz_options");

            entity.ToTable("quiz_options");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.OptionText).HasColumnName("option_text");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(false)
                .HasColumnName("is_correct");

            entity.HasOne(d => d.Question).WithMany(p => p.QuizOptions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_QuizOption_QuizQuestion");
        });

        modelBuilder.Entity<CourseSection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__course_sections");

            entity.ToTable("course_sections");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseSections)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_CourseSection_Course");
        });

        modelBuilder.Entity<SectionMaterial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__section_materials");

            entity.ToTable("section_materials");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.MaterialType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("material_type");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.OrderIndex).HasColumnName("order_index");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Section).WithMany(p => p.SectionMaterials)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SectionMaterial_CourseSection");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__students__3213E83FCF557172");

            entity.ToTable("students", tb => tb.HasTrigger("trg_AfterInsertStudent"));

            entity.HasIndex(e => e.StudentIdCode, "UQ__students__FBA5AC7318104181").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StudentIdCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("student_id_code");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Students)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Student_User");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__teachers__3213E83FBB5536FA");

            entity.ToTable("teachers", tb => tb.HasTrigger("trg_AfterInsertTeacher"));

            entity.HasIndex(e => e.TeacherIdCode, "UQ__teachers__6E31A32DBB74B00C").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TeacherIdCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("teacher_id_code");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Teacher_User");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transact__3213E83F80E266C6");

            entity.ToTable("transactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trans_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F18D72A87");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E616444F120B0").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.IsBanned)
                .HasDefaultValue(false)
                .HasColumnName("is_banned");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.Birthday)
                .HasColumnType("date")
                .HasColumnName("birthday");
            entity.Property(e => e.Bio)
                .HasColumnName("bio");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
        });

        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_sub__3213E83FEB6A363E");

            entity.ToTable("user_subscriptions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("end_date");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.Plan).WithMany(p => p.UserSubscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sub_Plan");

            entity.HasOne(d => d.Student).WithMany(p => p.UserSubscriptions)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sub_Student");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.ToTable("refresh_tokens");
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .HasColumnName("token");
            entity.Property(e => e.ExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("expiry_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRevoked)
                .HasDefaultValue(false)
                .HasColumnName("is_revoked");
            
            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RefreshToken_User");
        });

        modelBuilder.Entity<OtpCode>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.ToTable("otp_codes");
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Code)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.ExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("expiry_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsUsed)
                .HasDefaultValue(false)
                .HasColumnName("is_used");
        });

        modelBuilder.Entity<EmailVerificationToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.ToTable("email_verification_tokens");
            
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .HasColumnName("token");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.ExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("expiry_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_verified");
        });

        modelBuilder.Entity<PasswordResetCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__password_reset_codes");

            entity.ToTable("password_reset_codes");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");

            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("code");

            entity.Property(e => e.ExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("expiry_date");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.Property(e => e.IsUsed)
                .HasDefaultValue(false)
                .HasColumnName("is_used");

            entity.Property(e => e.AttemptCount)
                .HasDefaultValue(0)
                .HasColumnName("attempt_count");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__password_reset_tokens");

            entity.ToTable("password_reset_tokens");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");

            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .HasColumnName("token");

            entity.Property(e => e.ExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("expiry_date");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");

            entity.Property(e => e.IsUsed)
                .HasDefaultValue(false)
                .HasColumnName("is_used");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
