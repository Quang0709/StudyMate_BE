using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Service.DTOs;
using Service.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Implement;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool success, string message, List<CategoryDto>? data)> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();

            var categoryDtos = categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .OrderBy(c => c.Name)
                .ToList();

            return (true, "Categories retrieved successfully", categoryDtos);
        }
        catch (Exception ex)
        {
            return (false, $"Error retrieving categories: {ex.Message}", null);
        }
    }
}
