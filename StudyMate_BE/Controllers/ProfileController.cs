using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserService _userService;

    public ProfileController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> EditProfile(int id, EditProfileDto dto)
    {
        var userIdFromToken = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        if (id != userIdFromToken)
            return Forbid("You can only edit your own profile");

        var result = await _userService.EditProfileAsync(id, dto);
        if (!result)
            return NotFound("User not found");

        return Ok("Profile updated successfully");
    }
}
