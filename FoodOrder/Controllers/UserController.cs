using FoodOrder.Application.DTOs.Order;
using FoodOrder.Application.DTOs.Review;
using FoodOrder.Application.DTOs.User;
using FoodOrder.Application.Interfaces;
using FoodOrder.Domain.Entities;
using FoodOrder.Halpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FoodOrder.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repo;
        private readonly IDistributedCache _cache;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository repo, IDistributedCache cache, ILogger<UserController> logger)
        {
            _repo = repo;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<UserResponse>>> GetAll()
        {
            const string cacheKey = "users_all";

            var cachedData = await _cache.GetStringAsync(cacheKey);
            _logger.LogInformation("requested for all users");
            if (cachedData != null)
            {
                _logger.LogInformation("cache hit for key {cacheKey}",cacheKey);
                var cachedUsers = System.Text.Json.JsonSerializer
                    .Deserialize<List<UserResponse>>(cachedData);
                return Ok(cachedUsers);
                
            }
            else 
            {
                _logger.LogInformation("Cache miss for key {CacheKey}", cacheKey); 
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
            string cacheKey = $"user_{id}";
            var cached= await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Fetching user with ID {UserId}", id);
                var response= JsonSerializer.Deserialize<UserResponse>(cached);
                return Ok(response);

            }
            else
            {
                _logger.LogInformation("Cache miss for restaurant ID {RestaurantId}", id);
            }
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();
            var mapped = MapToResponse(existing);
            var serialized = JsonSerializer.Serialize(mapped);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            });
            _logger.LogInformation("Cached restaurant ID {RestaurantId}", id);

            return Ok(mapped);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponse>> Create(UserRequest userRequest)
        {
            var user = new User
            {
                Username = userRequest.Username,
                Email = userRequest.Email,
                FullName = userRequest.FullName,
                PhoneNumber = userRequest.PhoneNumber,
                Role = userRequest.IsOwner ? UserRole.Owner : UserRole.Customer,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRequest.Password),
                HashMethod = HashMethod.BCrypt
            };
            _logger.LogInformation("Creating new user with username {Username}", userRequest.Username);
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
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                existing.HashMethod = HashMethod.BCrypt;
            }
            _logger.LogInformation("Updating user with ID {UserId}", id);

            if (existing == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for update", id);
            }
            else if (existing.Id != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to update user {TargetUserId}", currentUserId, id);
            }

            await _repo.UpdateAsync(existing);
            await _cache.RemoveAsync("users_all");
            await _cache.RemoveAsync($"user_{id}");
            _logger.LogInformation("Invalidated caches for user ID {UserId}", id);
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
            _logger.LogInformation("Deleting user with ID {UserId}", id);

            if (existing == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for deletion", id);
            }
            else if (existing.Id != currentUserId)
            {
                _logger.LogWarning("User {CurrentUserId} attempted to delete user {TargetUserId}", currentUserId, id);
            }

            var deleted = await _repo.DeleteAsync(id);
            await _cache.RemoveAsync("users_all");
            await _cache.RemoveAsync($"user_{id}");
            _logger.LogInformation("Invalidated caches for user ID {UserId}", id);
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
                    Status = o.Status
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
