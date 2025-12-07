using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using FoodOrder.DTOs.User; 
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationRepository<User> _repo;

        public UserController(ApplicationRepository<User> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserResponse>>> GetAll()
        {
            var users = await _repo.GetAllAsync();
            return users.Select(MapToResponse).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetById(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return NotFound();
            return MapToResponse(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserResponse>> Create(User user)
        {
            
            var created = await _repo.AddAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, User user)
        {
            if (id != user.Id) return BadRequest();

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

           
            var currentUserId = GetCurrentUserId();
            if (existing.Id != currentUserId) return Forbid();

            await _repo.UpdateAsync(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
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
                Orders = user.Orders.Select(o => new UserOrderResponse
                {
                    Status = o.Status.ToString()
                }).ToList(),
                Reviews = user.Reviews.Select(r => new UserReviewResponse
                {
                    Comment = r.Comment,
                    Rating = r.Rating
                }).ToList()
            };
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
        }
    }
}
