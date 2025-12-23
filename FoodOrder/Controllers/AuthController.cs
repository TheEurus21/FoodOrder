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
    private readonly PasswordHasherFactory _passwordHasherFactory;

    public AuthController(ITokenService tokenService, IUserRepository repo, PasswordHasherFactory passwordHasherFactory)
    {
        _tokenService = tokenService;
        _repo = repo;
        _passwordHasherFactory = passwordHasherFactory;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(Login request)
    {
        var user = await _repo.GetByUsernameAsync(request.Username);
        if (user == null) return Unauthorized();

        var hasher = _passwordHasherFactory.GetHasher(user.HashMethod);

        if (!hasher.VerifyPassword(user, request.Password))
            return Unauthorized();

        if (user.HashMethod != HashMethod.BCrypt)
        {
            var newHasher = _passwordHasherFactory.GetHasher(HashMethod.BCrypt);
            user.PasswordHash = newHasher.HashPassword(request.Password);
            user.HashMethod = HashMethod.BCrypt;
            await _repo.UpdateAsync(user);
        }

        var token = _tokenService.GenerateToken(user);
        return Ok(new { token });
    }

}
