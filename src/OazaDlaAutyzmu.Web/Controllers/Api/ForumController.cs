using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OazaDlaAutyzmu.Application.Commands.Forum;
using OazaDlaAutyzmu.Application.Queries.Forum;
using OazaDlaAutyzmu.Application.DTOs;
using OazaDlaAutyzmu.Infrastructure.Data;
using OazaDlaAutyzmu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace OazaDlaAutyzmu.Web.Controllers.Api;

[ApiController]
[Route("api/v1/forum")]
[Produces("application/json")]
public class ForumController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ForumController> _logger;

    public ForumController(IMediator mediator, ApplicationDbContext context, ILogger<ForumController> logger)
    {
        _mediator = mediator;
        _context = context;
        _logger = logger;
    }

    private async Task<string> GenerateUniqueTopicSlugAsync(string title)
    {
        string Normalize(string input)
        {
            var s = input.ToLower().Replace(" ", "-")
                .Replace("ó", "o").Replace("ż", "z").Replace("ź", "z").Replace("ą", "a").Replace("ę", "e").Replace("ć", "c").Replace("ł", "l").Replace("ń", "n").Replace("ś", "s");
            var sb = new System.Text.StringBuilder();
            foreach (var ch in s)
            {
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '-')
                    sb.Append(ch);
            }
            return sb.ToString().Trim('-');
        }

        var baseSlug = Normalize(title);
        var slug = baseSlug;
        var i = 1;
        while (await _context.ForumTopics.AnyAsync(t => t.Slug == slug))
        {
            slug = baseSlug + "-" + i;
            i++;
        }

        return slug;
    }

    /// <summary>
    /// Get all forum categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var query = new GetForumCategoriesQuery();
        var categories = await _mediator.Send(query);
        return Ok(new { data = categories });
    }

    /// <summary>
    /// Get topics for a category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    [HttpGet("categories/{categoryId}/topics")]
    public async Task<IActionResult> GetTopics(int categoryId)
    {
        var query = new GetTopicsByCategoryQuery { CategoryId = categoryId };
        var topics = await _mediator.Send(query);
        return Ok(new { data = topics });
    }

    /// <summary>
    /// Get a single forum category by id
    /// </summary>
    [HttpGet("categories/{categoryId}")]
    public async Task<IActionResult> GetCategory(int categoryId)
    {
        var category = await _context.ForumCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
            return NotFound(new { message = $"Category with ID {categoryId} not found" });

        var result = new
        {
            id = category.Id,
            name = category.Name,
            slug = category.Slug,
            description = category.Description,
            sortOrder = category.SortOrder,
            createdAt = category.CreatedAt
        };

        return Ok(new { data = result });
    }

    /// <summary>
    /// Create a new forum category (Admin only)
    /// </summary>
    [HttpPost("categories")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Name is required" });

        // Generate unique slug
        var slug = await GenerateUniqueCategorySlugAsync(request.Name);

        var category = new ForumCategory
        {
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim(),
            SortOrder = request.SortOrder
        };

        _context.ForumCategories.Add(category);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error while creating category. Name={Name}", request.Name);
            // Likely unique constraint violation on Slug or other DB issue
            return Conflict(new { message = "Could not create category. Possible slug conflict or database error.", details = dbEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating category. Name={Name}", request.Name);
            return StatusCode(500, new { message = "An unexpected error occurred while creating the category.", details = ex.Message });
        }

        var created = new
        {
            id = category.Id,
            name = category.Name,
            slug = category.Slug,
            description = category.Description,
            sortOrder = category.SortOrder,
            createdAt = category.CreatedAt
        };

        // Return location pointing to the created category
        return CreatedAtAction(nameof(GetCategory), new { categoryId = category.Id }, new { data = created });
    }

    // Support query-style endpoint: /api/v1/forum/topics?categoryId=1
    [HttpGet("topics")]
    public async Task<IActionResult> GetTopicsByQuery([FromQuery] int? categoryId)
    {
        // If categoryId is not provided, return all topics
        if (!categoryId.HasValue || categoryId.Value <= 0)
        {
            var topics = await _context.ForumTopics
                .AsNoTracking()
                .Include(t => t.Category)
                .Include(t => t.Author)
                .Include(t => t.Posts)
                .ToListAsync();

            var topicDtos = topics.Select(t => new ForumTopicDto
            {
                Id = t.Id,
                CategoryId = t.CategoryId,
                CategoryName = t.Category?.Name ?? "Uncategorized",
                Title = t.Title,
                UserId = t.AuthorId,
                UserName = t.Author?.UserName ?? "Anonim",
                IsLocked = t.IsLocked,
                IsPinned = t.IsPinned,
                ViewCount = t.ViewCount,
                PostCount = t.Posts?.Count ?? 0,
                CreatedAt = t.CreatedAt,
                LatestPost = t.Posts?
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new ForumPostDto
                    {
                        Id = p.Id,
                        TopicId = p.TopicId,
                        TopicTitle = t.Title,
                        UserId = p.AuthorId,
                        UserName = p.Author?.UserName ?? "Anonim",
                        Content = p.Content,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .FirstOrDefault()
            })
            .OrderByDescending(t => t.IsPinned)
            .ThenByDescending(t => t.LatestPost?.CreatedAt ?? t.CreatedAt)
            .ToList();

            return Ok(new { data = topicDtos });
        }

        var query = new GetTopicsByCategoryQuery { CategoryId = categoryId.Value };
        var topicsByCategory = await _mediator.Send(query);
        return Ok(new { data = topicsByCategory });
    }

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;
}


    /// <summary>
    /// Get a specific topic with its posts
    /// </summary>
    /// <param name="topicId">Topic ID</param>
    [HttpGet("topics/{topicId}")]
    public async Task<IActionResult> GetTopic(int topicId)
    {
        var query = new GetTopicByIdQuery { Id = topicId };
        var topic = await _mediator.Send(query);

        if (topic == null)
        {
            return NotFound(new { message = $"Topic with ID {topicId} not found" });
        }

        return Ok(new { data = topic });
    }

    // TYMCZASOWO WYŁĄCZONE - używamy MVC endpoint zamiast API
    /*
    /// <summary>
    /// Create a new topic (requires authentication)
    /// </summary>
    /// <param name="request">Topic data</param>
    [HttpPost("topics")]
    [Authorize]
    public async Task<IActionResult> CreateTopic([FromBody] CreateTopicRequest request)
    {
        _logger.LogInformation("CreateTopic API called. User authenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            _logger.LogWarning("CreateTopic: User not authenticated or invalid user ID claim");
            return Unauthorized(new { message = "User not authenticated" });
        }

        _logger.LogInformation("CreateTopic: UserId={UserId}, Request received", userId);

        if (request == null)
        {
            _logger.LogWarning("CreateTopic: Request body is null");
            return BadRequest(new { message = "Request body is required" });
        }

        if (request.CategoryId <= 0)
        {
            _logger.LogWarning("CreateTopic: Invalid CategoryId={CategoryId}", request.CategoryId);
            return BadRequest(new { message = "categoryId must be a positive integer" });
        }

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length < 5)
        {
            _logger.LogWarning("CreateTopic: Invalid title. Length={Length}", request.Title?.Length ?? 0);
            return BadRequest(new { message = "Title is required and must be at least 5 characters" });
        }

        if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length < 10)
        {
            _logger.LogWarning("CreateTopic: Invalid content. Length={Length}", request.Content?.Length ?? 0);
            return BadRequest(new { message = "Content is required and must be at least 10 characters" });
        }

        // Ensure category exists
        var categoryExists = await _context.ForumCategories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            _logger.LogWarning("CreateTopic: Category with ID {CategoryId} does not exist", request.CategoryId);
            return BadRequest(new { message = $"Category with ID {request.CategoryId} does not exist" });
        }

        _logger.LogInformation("CreateTopic: Starting transaction for UserId={UserId}, CategoryId={CategoryId}, Title={Title}", 
            userId, request.CategoryId, request.Title);

        // Create topic and initial post inside a transaction
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var slug = await GenerateUniqueTopicSlugAsync(request.Title);

            var topic = new ForumTopic
            {
                CategoryId = request.CategoryId,
                AuthorId = userId,
                Title = request.Title.Trim(),
                Slug = slug,
                IsLocked = false,
                IsPinned = false,
                ViewCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.ForumTopics.Add(topic);
            await _context.SaveChangesAsync();

            var post = new ForumPost
            {
                TopicId = topic.Id,
                AuthorId = userId,
                Content = request.Content.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsApproved = true
            };

            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();

            // Update topic metadata
            topic.PostCount = 1;
            topic.LastPostAt = post.CreatedAt;
            topic.LastPostUserId = post.AuthorId;
            await _context.SaveChangesAsync();

            await tx.CommitAsync();

            _logger.LogInformation("CreateTopic: Topic created successfully. TopicId={TopicId}, UserId={UserId}", topic.Id, userId);

            return CreatedAtAction(nameof(GetTopic), new { topicId = topic.Id }, new { data = new { id = topic.Id, message = "Topic created successfully" } });
        }
        catch (DbUpdateException dbEx)
        {
            await tx.RollbackAsync();
            _logger.LogError(dbEx, "Database error while creating topic. CategoryId={CategoryId}, UserId={UserId}", request?.CategoryId, userId);
            return StatusCode(500, new { message = "Database error while creating topic", details = dbEx.Message });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Unexpected error while creating topic. CategoryId={CategoryId}, UserId={UserId}", request?.CategoryId, userId);
            return StatusCode(500, new { message = "An unexpected error occurred while creating the topic", details = ex.Message });
        }
    }
    */

    /// <summary>
    /// Create a post in a topic (requires authentication)
    /// </summary>
    /// <param name="topicId">Topic ID</param>
    /// <param name="request">Post data</param>
    [HttpPost("topics/{topicId}/posts")]
    [Authorize]
    public async Task<IActionResult> CreatePost(int topicId, [FromBody] CreatePostRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var command = new CreatePostCommand
        {
            TopicId = topicId,
            UserId = userId,
            Content = request.Content
        };

        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Post created successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task<string> GenerateUniqueCategorySlugAsync(string name)
    {
        string Normalize(string input)
        {
            var s = input.ToLower().Replace(" ", "-")
                .Replace("ó", "o").Replace("ż", "z").Replace("ź", "z").Replace("ą", "a").Replace("ę", "e").Replace("ć", "c").Replace("ł", "l").Replace("ń", "n").Replace("ś", "s");
            var sb = new System.Text.StringBuilder();
            foreach (var ch in s)
            {
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '-')
                    sb.Append(ch);
            }
            return sb.ToString().Trim('-');
        }

        var baseSlug = Normalize(name);
        var slug = baseSlug;
        var i = 1;
        while (await _context.ForumCategories.AnyAsync(c => c.Slug == slug))
        {
            slug = baseSlug + "-" + i;
            i++;
        }

        return slug;
    }
}

public class CreateTopicRequest
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class CreatePostRequest
{
    public string Content { get; set; } = string.Empty;
}
