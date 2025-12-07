using FoodOrder.DTOs.User;
using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/reviews")]
public class ReviewController : ControllerBase
{
    private readonly ApplicationRepository<Review> _repo;

    public ReviewController(ApplicationRepository<Review> repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserReviewResponse>>> GetAll()
    {
        var reviews = await _repo.GetAllAsync();
        return reviews.Select(MapToResponse).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<UserReviewResponse>> Create(Review review)
    {
        review.UserId = GetCurrentUserId();

        var created = await _repo.AddAsync(review);
        return Ok(MapToResponse(created));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();

        var currentUserId = GetCurrentUserId();
        if (existing.UserId != currentUserId) return Forbid(); 

        var deleted = await _repo.DeleteAsync(id);
        if (!deleted) return NotFound();

        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, Review review)
    {
        if (id != review.Id) return BadRequest();

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();

        var currentUserId = GetCurrentUserId();
        if (existing.UserId != currentUserId) return Forbid(); 

        await _repo.UpdateAsync(review);
        return NoContent();
    }

    private UserReviewResponse MapToResponse(Review review)
    {
        return new UserReviewResponse
        {
            Comment = review.Comment,
            FoodId = review.FoodId,
            Rating = review.Rating
        };
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
    }
}
