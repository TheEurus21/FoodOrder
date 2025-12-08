using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using FoodOrder.DTOs.User;
using FoodOrder.DTOs.Order;
using FoodOrder.DTOs.Review;
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
        public async Task<ActionResult<UserResponse>> Create(UserRequest userrequest)
        {
            var user = new User
            {
                Username = userrequest.Username,
                Email = userrequest.Email,
                Password = userrequest.Password, 
                FullName = userrequest.FullName,
                PhoneNumber = userrequest.PhoneNumber,
                IsOwner = userrequest.IsOwner
            };

            var created = await _repo.AddAsync(user);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
        }

        [HttpPut("{id}")]
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
            existing.IsOwner = request.IsOwner;
            await _repo.UpdateAsync(existing);
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
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
        }
    }
}
