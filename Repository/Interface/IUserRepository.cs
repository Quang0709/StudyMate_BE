using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Repository.Model;

namespace Repository.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();

}
