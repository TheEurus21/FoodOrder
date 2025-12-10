using FoodOrder.DTOs.Review;
using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/reviews")]
[Authorize]
public class ReviewController : CommonController
{
    private readonly ApplicationRepository<Review> _repo;

    public ReviewController(ApplicationRepository<Review> repo)
    {
        _repo = repo;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ReviewResponse>>> GetAll()
    {
        var reviews = await _repo.GetAllAsync();
        return reviews.Select(MapToResponse).ToList();
    }

    [HttpGet("{id}")]
    [AllowAnonymous] // 👀 Public can view a single review
    public async Task<ActionResult<ReviewResponse>> GetById(int id)
    {
        var review = await _repo.GetByIdAsync(id);
        if (review == null) return NotFound();
        return MapToResponse(review);
    }
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<ReviewResponse>> Create(ReviewRequest request)
    {
        var review = new Review
        {
            FoodName = request.FoodName,
            Rating = request.Rating,
            Comment = request.Comment,
            UserId = GetCurrentUserId(),   
            CreatedAt = DateTime.UtcNow   
        };

        var created = await _repo.AddAsync(review);
        return Ok(MapToResponse(created));
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = "Customer")]
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
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult> Update(int id, ReviewRequest request)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();

        var currentUserId = GetCurrentUserId();
        if (existing.UserId != currentUserId) return Forbid();
        existing.Rating = request.Rating;
        existing.Comment = request.Comment;

        await _repo.UpdateAsync(existing);
        return NoContent();
    }

    private ReviewResponse MapToResponse(Review review)
    {
        return new ReviewResponse
        {
            Comment = review.Comment,
            FoodId = review.FoodId,
            Rating = review.Rating
        };
    }

}
