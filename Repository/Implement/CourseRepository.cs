using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Interface;
using Repository.Model;

namespace Repository.Implement;

public class CourseRepository : ICourseRepository
{
    private readonly StudyMateContext _context;

    public CourseRepository(StudyMateContext context)
    {
        _context = context;
    }

    public async Task<List<Course>> SearchAsync(
        string? title,
        int? categoryId,
        string? teacherName)
    {
        IQueryable<Course> query = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(c => c.Title.Contains(title));

        if (categoryId.HasValue)
            query = query.Where(c => c.CategoryId == categoryId);

        if (!string.IsNullOrWhiteSpace(teacherName))
            query = query.Where(c =>
                c.Teacher.User.FullName.Contains(teacherName));

        return await query.ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(int courseId)
    {
        return await _context.Courses
            .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
            .FirstOrDefaultAsync(c => c.Id == courseId && !c.IsDeleted);
    }

    public async Task UpdateAsync(Course course)
    {
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }
}
