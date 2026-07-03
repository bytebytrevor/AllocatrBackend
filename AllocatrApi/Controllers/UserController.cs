using AllocatrApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserManager<AllocatrUser> _userManager;

    public UserController(UserManager<AllocatrUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpPatch("me/become-allocat")]
    public async Task<IActionResult> BecomeAllocat()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        if (user.IsAllocat)
            return BadRequest("User is already and allocat.");

        user.IsAllocat = true;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return NoContent();
    }
}