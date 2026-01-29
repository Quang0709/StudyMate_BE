using Repository.Interface;
using Repository.Model;
using Service.DTOs;
using Service.Interfaces;


namespace Service.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork; 

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _unitOfWork.Users.GetAllAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _unitOfWork.Users.GetByIdAsync(id);
    }
    public async Task<bool> EditProfileAsync(int userId, EditProfileDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            user.FullName = dto.FullName;

        if (!string.IsNullOrWhiteSpace(dto.Email))
            user.Email = dto.Email;

        if (dto.Birthday.HasValue)
            user.Birthday = dto.Birthday;

        if (dto.Bio != null)
            user.Bio = dto.Bio;

        if (!string.IsNullOrWhiteSpace(dto.AvatarUrl))
            user.AvatarUrl = dto.AvatarUrl;

        if (!string.IsNullOrWhiteSpace(dto.Address))
            user.Address = dto.Address;

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BanUserAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return false;

        user.IsBanned = true;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnbanUserAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) return false;

        user.IsBanned = false;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
