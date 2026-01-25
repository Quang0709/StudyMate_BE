using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Repository.Model;
using Service.DTOs;

namespace Service.Interfaces;

public interface IUserService
{
    //admin
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);

    Task<bool> BanUserAsync(int id);
    Task<bool> UnbanUserAsync(int id);



    //user
    Task<bool> EditProfileAsync(int userId, EditProfileDto dto);


}
