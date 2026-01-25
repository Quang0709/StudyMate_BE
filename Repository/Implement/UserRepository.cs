using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Model;

namespace Repository.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbContext _context;

    public UserRepository(DbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Set<User>()
            .AsNoTracking()
            .ToListAsync();
    }
}
