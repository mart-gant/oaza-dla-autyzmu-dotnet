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
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<ForumController> _logger;

    public ForumController(
        IMediator mediator, 
        IValidator<CreateTopicCommand> topicValidator,
        IValidator<CreatePostCommand> postValidator,
        IHtmlSanitizerService htmlSanitizer,
        IContentModerationService contentModeration,
        INotificationService notificationService,
        ApplicationDbContext context,
        ILogger<ForumController> logger)
    {
        _mediator = mediator;
        _topicValidator = topicValidator;
        _postValidator = postValidator;
        _htmlSanitizer = htmlSanitizer;
        _contentModeration = contentModeration;
        _notificationService = notificationService;
        _context = context;
        _logger = logger;
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
        _logger.LogInformation("======= MVC CreateTopic POST HIT! =======");
        _logger.LogInformation("CategoryId: {CategoryId}, Title: {Title}, Content length: {ContentLength}", 
            categoryId, title, content?.Length ?? 0);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            _logger.LogWarning("MVC CreateTopic: Unauthorized - no valid userId");
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

    // Admin UI: create forum category (Razor view)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult CreateCategory()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string name, string? description, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("", "Nazwa kategorii jest wymagana");
            return View();
        }

        var slug = await GenerateUniqueCategorySlugAsync(name);

        var category = new OazaDlaAutyzmu.Domain.Entities.ForumCategory
        {
            Name = name.Trim(),
            Slug = slug,
            Description = description?.Trim(),
            SortOrder = sortOrder
        };

        _context.ForumCategories.Add(category);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Kategoria została utworzona.";
        return RedirectToAction(nameof(Index));
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
