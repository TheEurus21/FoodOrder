using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using FoodOrder.DTOs.User;
using FoodOrder.DTOs.Order;
using FoodOrder.DTOs.Review;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : CommonController
    {
        private readonly ApplicationRepository<User> _repo;

        public UserController(ApplicationRepository<User> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<UserResponse>>> GetAll()
        {
            var users = await _repo.GetAllAsync();
            return users.Select(MapToResponse).ToList();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserResponse>> GetById(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return NotFound();
            return MapToResponse(user);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<UserResponse>> Create(UserRequest userRequest)
        {
            var user = new User
            {
                Username = userRequest.Username,
                Email = userRequest.Email,
                Password = userRequest.Password,
                FullName = userRequest.FullName,
                PhoneNumber = userRequest.PhoneNumber,
                Role = userRequest.IsOwner ? UserRole.Owner : UserRole.Customer
            };

            var created = await _repo.AddAsync(user);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
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

  
    }
}
