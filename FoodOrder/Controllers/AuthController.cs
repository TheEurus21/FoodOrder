using Azure.Core;
using FoodOrder.DTOs.Auth;
using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly ApplicationRepository<User> _repo;

    public AuthController(TokenService tokenService, ApplicationRepository<User> repo)
    {
        _tokenService = tokenService;
        _repo = repo;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login( Login request)
    {
        var user = _repo.GetAllAsync()
    .Result.FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);

        if (user == null) return Unauthorized();

        var token = _tokenService.GenerateToken(user);
        return Ok(new { token });
    }
}
