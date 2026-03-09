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
        return await _context.UserSubscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s =>
                s.StudentId == userId &&
                s.IsActive &&
                s.EndDate >= DateTime.UtcNow &&
                s.Plan.TargetRole == role
            );
    }
}