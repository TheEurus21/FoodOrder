using FoodOrder.Application.DTOs.Order;
using FoodOrder.Application.DTOs.Review;
using FoodOrder.Application.DTOs.User;
using FoodOrder.Application.Interfaces;
using FoodOrder.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace FoodOrder.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repo;
        private readonly IDistributedCache _cache;

        public UserController(IUserRepository repo, IDistributedCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<UserResponse>>> GetAll()
        {
            const string cacheKey = "users_all";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedUsers = System.Text.Json.JsonSerializer
                    .Deserialize<List<UserResponse>>(cachedData);
                return Ok(cachedUsers);
            }

            var users = await _repo.GetAllAsync();
            var response = users.Select(MapToResponse).ToList();

            var serialized = System.Text.Json.JsonSerializer.Serialize(response);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserResponse>> GetById(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            return Ok(MapToResponse(existing));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponse>> Create(UserRequest userRequest)
        {
            var passwordHasher = new PasswordHasher<User>();
            var user = new User
            {
                Username = userRequest.Username,
                Email = userRequest.Email,
                FullName = userRequest.FullName,
                PhoneNumber = userRequest.PhoneNumber,
                Role = userRequest.IsOwner ? UserRole.Owner : UserRole.Customer,
                PasswordHash = passwordHasher.HashPassword(new User(), userRequest.Password)
            };

            var created = await _repo.AddAsync(user);
            return Created($"api/users/{created.Id}", MapToResponse(created));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(int id, UserRequest request)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            if (existing.Id != currentUserId) return Forbid();

            existing.Username = request.Username;
            existing.Email = request.Email;
            existing.FullName = request.FullName;
            existing.PhoneNumber = request.PhoneNumber;
            existing.Role = request.IsOwner ? UserRole.Owner : UserRole.Customer;

            if (!string.IsNullOrEmpty(request.Password))
            {
                var passwordHasher = new PasswordHasher<User>();
                existing.PasswordHash = passwordHasher.HashPassword(existing, request.Password);
            }

            await _repo.UpdateAsync(existing);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            if (existing.Id != currentUserId) return Forbid();

            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();

            return NoContent();
        }

        private UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                Username = user.Username,
                Email = user.Email,
                Orders = user.Orders.Select(o => new OrderResponse
                {
                    Status = o.Status.ToString()
                }).ToList(),
                Reviews = user.Reviews.Select(r => new ReviewResponse
                {
                    Comment = r.Comment,
                    Rating = r.Rating
                }).ToList()
            };
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
    }
}
