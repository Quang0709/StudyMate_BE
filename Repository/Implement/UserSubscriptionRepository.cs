using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Interface;
using Repository.Model;

public class UserSubscriptionRepository : IUserSubscriptionRepository
{
    private readonly StudyMateContext _context;

    public UserSubscriptionRepository(StudyMateContext context)
    {
        _context = context;
    }

    public async Task<UserSubscription?>
        GetActiveSubscriptionAsync(int userId, string role)
    {
        var normalizedRole = role?.Trim().ToLower();

        if (normalizedRole != "student" && normalizedRole != "teacher")
        {
            return null;
        }

        int? studentId = null;
        int? teacherId = null;

        if (normalizedRole == "student")
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null)
            {
                return null;
            }

            studentId = student.Id;
        }
        else
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null)
            {
                return null;
            }

            teacherId = teacher.Id;
        }

        return await _context.UserSubscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s =>
                ((normalizedRole == "student" && s.StudentId == studentId) ||
                 (normalizedRole == "teacher" && s.TeacherId == teacherId)) &&
                s.IsActive &&
                s.EndDate >= DateTime.UtcNow &&
                s.Plan.TargetRole == normalizedRole
            );
    }
}