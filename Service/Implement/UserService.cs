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


       
                if (dto.FullName != null)
                    user.FullName = dto.FullName;


        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var email = dto.Email.Trim().ToLower();

            if (!email.EndsWith("@gmail.com"))
            {
                throw new ArgumentException("Email must end with @gmail.com");
            }

            user.Email = email;
        }
        /*
                       if (dto.PhoneNumber != null)
                           user.PhoneNumber = dto.PhoneNumber;

                       if (dto.AvatarUrl != null)
                           user.AvatarUrl = dto.AvatarUrl;

                       if (dto.Address != null)
                           user.Address = dto.Address;

                       if (dto.DateOfBirth.HasValue)
                           user.DateOfBirth = dto.DateOfBirth;
               */

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
