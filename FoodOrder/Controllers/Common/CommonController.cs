using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public abstract class CommonController : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    protected string? GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value;
    }

    protected string? GetUserName()
    {
        return User.Identity?.Name;
    }
}
