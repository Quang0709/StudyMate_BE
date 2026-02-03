namespace Service.DTOs;

public class SearchCourseDto
{
    public string? Title { get; set; }
    public int? CategoryId { get; set; }
    public string? TeacherName { get; set; } // FullName từ User
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
