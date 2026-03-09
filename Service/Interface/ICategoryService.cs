using Service.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Service.Interface;

public interface ICategoryService
{
    Task<(bool success, string message, List<CategoryDto>? data)> GetAllCategoriesAsync();
}
