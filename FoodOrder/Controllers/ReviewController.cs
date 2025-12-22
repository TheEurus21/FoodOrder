using FoodOrder.Application.DTOs.Review;
using FoodOrder.Application.Interfaces;
using FoodOrder.Domain.Entities;
using FoodOrder.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

[ApiController]
[Route("api/reviews")]
[Authorize]
public class ReviewController : CommonController
{
    private readonly IReviewRepository _repo;
    private readonly IDistributedCache _cache;

    public ReviewController(IReviewRepository repo, IDistributedCache cache)
    {
        _repo = repo;
        _cache = cache;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ReviewResponse>>> GetAll()
    {
        const string cacheKey = "reviews_all";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData != null)
        {
            var cachedReviews = System.Text.Json.JsonSerializer
                .Deserialize<List<ReviewResponse>>(cachedData);
        }

        var reviews = await _repo.GetAllAsync();
        var response = reviews.Select(MapToResponse).ToList();

        var serialized = System.Text.Json.JsonSerializer.Serialize(response);
        await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        });

        return Ok(response);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ReviewResponse>>GetById(int id)
    {
        var existing = await _repo.GetByIdAsync(id);

        if (existing == null)return NotFound();

        return Ok(existing);

    }

  
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<ReviewResponse>> Create(ReviewRequest request)
    {
        var review = new Review
        {
            FoodId = request.FoodId,
            Rating = request.Rating,
            Comment = request.Comment,
            UserId = GetCurrentUserId(),   
            CreatedAt = DateTime.UtcNow   
        };

        var created = await _repo.AddAsync(review);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult> Update(int id, ReviewRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var existingReview = await _repo.GetByIdAsync(id);
        if (existingReview == null) return NotFound();
        if (existingReview.UserId != currentUserId)
            return Forbid();
        existingReview.FoodId = request.FoodId;
        existingReview.Rating = request.Rating;
        existingReview.Comment = request.Comment;
        await _repo.UpdateAsync(existingReview);
        return NoContent();
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();

        var existingReview = await _repo.GetByIdAsync(id);
        if (existingReview == null) return NotFound();
        if(existingReview.UserId != currentUserId) return Forbid();
        var deleted = await _repo.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
    private ReviewResponse MapToResponse(Review review)
    {
        return new ReviewResponse
        {
            FoodId = review.FoodId,
            Rating = review.Rating,
            Comment = review.Comment,
            UserId = review.UserId,
            CreatedAt = review.CreatedAt
        };
    }

}
