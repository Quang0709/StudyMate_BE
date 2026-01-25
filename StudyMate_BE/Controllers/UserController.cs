using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Roles = "Admin")] // 🔐 CHỈ ADMIN
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET api/users
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // GET api/users/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound($"User with id = {id} not found");

        return Ok(user);
    }

    // PUT api/users/{id}/ban
    [HttpPut("{id:int}/ban")]
    public async Task<IActionResult> BanUser(int id)
    {
        var result = await _userService.BanUserAsync(id);
        if (!result)
            return NotFound($"User with id = {id} not found");

        return Ok("User has been banned");
    }

    // PUT api/users/{id}/unban
    [HttpPut("{id:int}/unban")]
    public async Task<IActionResult> UnbanUser(int id)
    {
        var result = await _userService.UnbanUserAsync(id);
        if (!result)
            return NotFound($"User with id = {id} not found");

        return Ok("User has been unbanned");
    }
}
