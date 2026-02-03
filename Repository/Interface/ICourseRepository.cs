using Repository.Model;

namespace Repository.Interface;

public interface ICourseRepository
{
    Task<List<Course>> SearchAsync(
        string? title,
        int? categoryId,
        string? teacherName,
        decimal? minPrice,
        decimal? maxPrice
    );
    Task<Course?> GetByIdAsync(int courseId);
    Task UpdateAsync(Course course);
}
