using FoodOrder.DTOs.Review;
using FoodOrder.Models;
using FoodOrder.Repositories.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    [HttpDelete("latest")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult> DeleteLatest()
    {
        var currentUserId = GetCurrentUserId();

        var existing = await _repo.GetLatestReviewByUserAsync(currentUserId);
        if (existing == null) return NotFound();

        var deleted = await _repo.DeleteAsync(existing.Id);
        if (!deleted) return NotFound();

        return NoContent();
    }
    [HttpPut("{restaurantName}")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult> Update(string restaurantName, ReviewRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var existing = await _repo.GetByRestaurantNameAndUserAsync(restaurantName, currentUserId);

        if (existing == null) return NotFound();

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
