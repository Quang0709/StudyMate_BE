using Service.DTOs;
using System.Threading.Tasks;

namespace Service.Interface;

public interface ICourseService
{
    Task<(bool Success, string Message, object? Data)> CreateCourseAsync(int teacherId, CreateCourseDto createCourseDto);
}
