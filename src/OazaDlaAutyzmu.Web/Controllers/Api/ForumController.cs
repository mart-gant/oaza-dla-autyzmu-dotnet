using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OazaDlaAutyzmu.Application.Commands.Forum;
using OazaDlaAutyzmu.Application.Queries.Forum;
using System.Security.Claims;

namespace OazaDlaAutyzmu.Web.Controllers.Api;

[ApiController]
[Route("api/v1/forum")]
[Produces("application/json")]
public class ForumController : ControllerBase
{
    private readonly IMediator _mediator;

    public ForumController(IMediator mediator)
    {
        _mediator = mediator;
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
