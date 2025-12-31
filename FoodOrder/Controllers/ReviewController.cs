using FoodOrder.Application.DTOs.Review;
using FoodOrder.Application.Interfaces;
using FoodOrder.Domain.Entities;
using FoodOrder.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

[ApiController]
[Route("api/reviews")]
[Authorize]
public class ReviewController : CommonController
{
    private readonly IReviewRepository _repo;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(IReviewRepository repo, IDistributedCache cache,ILogger<ReviewController>logger)
    {
        _repo = repo;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ReviewResponse>>> GetAll()
    {
        const string cacheKey = "reviews_all";
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (cachedData != null)
        {
            _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
            var cachedReviews = JsonSerializer.Deserialize<List<ReviewResponse>>(cachedData);
            return Ok(cachedReviews);
        }

        _logger.LogInformation("Cache miss for key {CacheKey}", cacheKey);

        var reviews = await _repo.GetAllAsync();
        var response = reviews.Select(MapToResponse).ToList();

        var serialized = JsonSerializer.Serialize(response);
        await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        });

        _logger.LogInformation("Cached {Count} reviews with key {CacheKey}", response.Count, cacheKey);

        return Ok(response);
    }
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ReviewResponse>> GetById(int id)
    {
        string cacheKey = $"review_{id}";
        var cached = await _cache.GetStringAsync(cacheKey);

        if (cached != null)
        {
            _logger.LogInformation("Cache hit for review ID {ReviewId}", id);
            var response = JsonSerializer.Deserialize<ReviewResponse>(cached);
            return Ok(response);
        }

        _logger.LogInformation("Cache miss for review ID {ReviewId}", id);

        var existing = await _repo.GetByIdAsync(id);
        if (existing == null)
        {
            _logger.LogWarning("Review with ID {ReviewId} not found", id);
            return NotFound();
        }

        var mapped = MapToResponse(existing);
        var serialized = JsonSerializer.Serialize(mapped);

        await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        });

        _logger.LogInformation("Cached review ID {ReviewId}", id);

        return Ok(mapped);
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
        await _cache.RemoveAsync("reviews_all");
        await _cache.RemoveAsync($"review_{id}");
        _logger.LogInformation("Invalidated caches for review ID {ReviewId}", id);

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
        await _cache.RemoveAsync("reviews_all");
        await _cache.RemoveAsync($"review_{id}");
        _logger.LogInformation("Invalidated caches for review ID {ReviewId}", id);

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
