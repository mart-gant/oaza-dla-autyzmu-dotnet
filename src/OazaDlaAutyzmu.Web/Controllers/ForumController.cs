using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OazaDlaAutyzmu.Application.Commands.Forum;
using OazaDlaAutyzmu.Application.Queries.Forum;
using OazaDlaAutyzmu.Infrastructure.Data;
using OazaDlaAutyzmu.Infrastructure.Services;
using OazaDlaAutyzmu.Web.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace OazaDlaAutyzmu.Web.Controllers;

public class ForumController : Controller
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateTopicCommand> _topicValidator;
    private readonly IValidator<CreatePostCommand> _postValidator;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IContentModerationService _contentModeration;
    private readonly INotificationService _notificationService;
    private readonly ApplicationDbContext _context;

    public ForumController(
        IMediator mediator, 
        IValidator<CreateTopicCommand> topicValidator,
        IValidator<CreatePostCommand> postValidator,
        IHtmlSanitizerService htmlSanitizer,
        IContentModerationService contentModeration,
        INotificationService notificationService,
        ApplicationDbContext context)
    {
        _mediator = mediator;
        _topicValidator = topicValidator;
        _postValidator = postValidator;
        _htmlSanitizer = htmlSanitizer;
        _contentModeration = contentModeration;
        _notificationService = notificationService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var query = new GetForumCategoriesQuery();
        var categories = await _mediator.Send(query);
        return View(categories);
    }

    [HttpGet]
    public async Task<IActionResult> Category(int id)
    {
        var query = new GetTopicsByCategoryQuery { CategoryId = id };
        var topics = await _mediator.Send(query);

        if (topics.Count > 0)
            ViewBag.CategoryName = topics[0].CategoryName;
        else
        {
            var categoryQuery = new GetForumCategoriesQuery();
            var categories = await _mediator.Send(categoryQuery);
            var category = categories.FirstOrDefault(c => c.Id == id);
            ViewBag.CategoryName = category?.Name ?? "Kategoria";
        }

        ViewBag.CategoryId = id;
        return View(topics);
    }

    [HttpGet]
    public async Task<IActionResult> Topic(int id)
    {
        var topicQuery = new GetTopicByIdQuery { Id = id };
        var topic = await _mediator.Send(topicQuery);

        if (topic == null)
            return NotFound();

        var postsQuery = new GetPostsByTopicQuery { TopicId = id };
        var posts = await _mediator.Send(postsQuery);

        ViewBag.Posts = posts;
        return View(topic);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> CreateTopic(int categoryId)
    {
        ViewBag.CategoryId = categoryId;
        
        var categoryQuery = new GetForumCategoriesQuery();
        var categories = await _mediator.Send(categoryQuery);
        var category = categories.FirstOrDefault(c => c.Id == categoryId);
        ViewBag.CategoryName = category?.Name ?? "Kategoria";

        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTopic(int categoryId, string title, string content)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var command = new CreateTopicCommand
        {
            CategoryId = categoryId,
            UserId = userId,
            Title = _htmlSanitizer.Sanitize(title),
            Content = _htmlSanitizer.Sanitize(content)
        };

        var validationResult = await _topicValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            ViewBag.CategoryId = categoryId;
            
            var categoryQuery = new GetForumCategoriesQuery();
            var categories = await _mediator.Send(categoryQuery);
            var category = categories.FirstOrDefault(c => c.Id == categoryId);
            ViewBag.CategoryName = category?.Name ?? "Kategoria";
            
            return View();
        }

        // Content moderation - check for profanity
        if (_contentModeration.ContainsProfanity(command.Title) || _contentModeration.ContainsProfanity(command.Content))
        {
            TempData["ErrorMessage"] = "Twój temat zawiera niedozwolone treści i nie może zostać opublikowany.";
            return RedirectToAction("Index", new { categoryId });
        }

        var topicId = await _mediator.Send(command);
        return RedirectToAction(nameof(Topic), new { id = topicId });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(int topicId, string content)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var command = new CreatePostCommand
        {
            TopicId = topicId,
            UserId = userId,
            Content = _htmlSanitizer.Sanitize(content)
        };

        var validationResult = await _postValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            TempData["ErrorMessage"] = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Topic), new { id = topicId });
        }

        // Content moderation - check for profanity
        if (_contentModeration.ContainsProfanity(command.Content))
        {
            TempData["ErrorMessage"] = "Twój post zawiera niedozwolone treści i nie może zostać opublikowany.";
            return RedirectToAction(nameof(Topic), new { id = topicId });
        }

        await _mediator.Send(command);

        // Notify topic author about new reply (if not the same user)
        var topic = await _context.ForumTopics.FindAsync(topicId);
        if (topic != null && topic.AuthorId != userId)
        {
            await _notificationService.CreateNotificationAsync(
                topic.AuthorId,
                "TopicReply",
                "Nowa odpowiedź w Twoim temacie",
                $"Ktoś dodał odpowiedź do Twojego tematu: {topic.Title}",
                $"/Forum/Topic/{topicId}");
        }

        return RedirectToAction(nameof(Topic), new { id = topicId });
    }
}
