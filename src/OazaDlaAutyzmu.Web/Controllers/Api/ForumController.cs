using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OazaDlaAutyzmu.Application.Commands.Forum;
using OazaDlaAutyzmu.Application.Queries.Forum;
using OazaDlaAutyzmu.Infrastructure.Data;
using OazaDlaAutyzmu.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace OazaDlaAutyzmu.Web.Controllers.Api;

[ApiController]
[Route("api/v1/forum")]
[Produces("application/json")]
public class ForumController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _context;

    public ForumController(IMediator mediator, ApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
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
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, new { data = new { id = category.Id, message = "Category created" } });
    }

    // Support query-style endpoint: /api/v1/forum/topics?categoryId=1
    [HttpGet("topics")]
    public async Task<IActionResult> GetTopicsByQuery([FromQuery] int categoryId)
    {
        var query = new GetTopicsByCategoryQuery { CategoryId = categoryId };
        var topics = await _mediator.Send(query);
        return Ok(new { data = topics });
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

    /// <summary>
    /// Create a new topic (requires authentication)
    /// </summary>
    /// <param name="request">Topic data</param>
    [HttpPost("topics")]
    [Authorize]
    public async Task<IActionResult> CreateTopic([FromBody] CreateTopicRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var command = new CreateTopicCommand
        {
            CategoryId = request.CategoryId,
            UserId = userId,
            Title = request.Title,
            Content = request.Content
        };

        try
        {
            var topicId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetTopic), new { topicId }, 
                new { data = new { id = topicId, message = "Topic created successfully" } });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

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
