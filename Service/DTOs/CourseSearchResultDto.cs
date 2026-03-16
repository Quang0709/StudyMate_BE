namespace Service.DTOs;

public class CourseSearchResultDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? CategoryName { get; set; }
    public string TeacherFullName { get; set; } = null!;
}
