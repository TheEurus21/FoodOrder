using FoodOrder.Application.DTOs.Auth;
using FoodOrder.Application.Interfaces;
using FoodOrder.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _repo;

    public AuthController(ITokenService tokenService, IUserRepository repo)
    {
        _tokenService = tokenService;
        _repo = repo;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(Login request)
    {
        var user = await _repo.GetByUsernameAsync(request.Username);

        if (user == null) return Unauthorized();

        var passwordHasher = new PasswordHasher<User>();
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
            return Unauthorized();

        var token = _tokenService.GenerateToken(user);
        return Ok(new { token });
    }
}
