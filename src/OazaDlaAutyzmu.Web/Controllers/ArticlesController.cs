using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Domain.Entities;
using OazaDlaAutyzmu.Infrastructure.Data;
using System.Security.Claims;

namespace OazaDlaAutyzmu.Web.Controllers;

public class ArticlesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ArticlesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Debug endpoint (Admin only) to list articles and their slugs/status. Remove or protect further in production.
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DebugList()
    {
        var list = await _context.Articles
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new { a.Id, a.Title, a.Slug, Status = a.Status.ToString(), a.PublishedAt })
            .ToListAsync();

        return Json(list);
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, int? excludeArticleId = null)
    {
        string Normalize(string input)
        {
            var s = input.ToLower()
                .Replace(" ", "-")
                .Replace("ó", "o").Replace("ż", "z").Replace("ź", "z").Replace("ą", "a").Replace("ę", "e").Replace("ć", "c").Replace("ł", "l").Replace("ń", "n").Replace("ś", "s");
            // Remove invalid chars
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
        while (await _context.Articles.AnyAsync(a => a.Slug == slug && (!excludeArticleId.HasValue || a.Id != excludeArticleId.Value)))
        {
            slug = baseSlug + "-" + i;
            i++;
        }

        return slug;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(int? categoryId, int pageNumber = 1)
    {
        var query = _context.Articles
            .AsNoTracking()
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Where(a => a.Status == ArticleStatus.Published);

        if (categoryId.HasValue)
            query = query.Where(a => a.CategoryId == categoryId.Value);

        var pageSize = 10;
        var totalCount = await query.CountAsync();
        var articles = await query
            .OrderByDescending(a => a.PublishedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Categories = await _context.ArticleCategories.OrderBy(c => c.Name).ToListAsync();
        ViewBag.PageNumber = pageNumber;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.SelectedCategory = categoryId;

        return View(articles);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(string slug)
    {
        var article = await _context.Articles
            .AsNoTracking()
            .Include(a => a.Author)
            .Include(a => a.Category)
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Slug == slug && a.Status == ArticleStatus.Published);

        if (article == null)
            return NotFound();

        // Increment view count
        var trackingArticle = await _context.Articles.FindAsync(article.Id);
        if (trackingArticle != null)
        {
            trackingArticle.ViewCount++;
            await _context.SaveChangesAsync();
        }

        return View(article);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _context.ArticleCategories.OrderBy(c => c.Name).ToListAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string title, string content, string? excerpt, int categoryId, bool publish)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            ModelState.AddModelError("", "Tytuł i treść są wymagane");
            ViewBag.Categories = await _context.ArticleCategories.OrderBy(c => c.Name).ToListAsync();
            return View();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userId, out var authorId))
            return Unauthorized();

        var category = await _context.ArticleCategories.FindAsync(categoryId);
        if (category == null)
        {
            ModelState.AddModelError("", "Wybrana kategoria nie istnieje");
            ViewBag.Categories = await _context.ArticleCategories.OrderBy(c => c.Name).ToListAsync();
            return View();
        }

        // Generate unique slug (avoid UNIQUE constraint violation)
        var slug = await GenerateUniqueSlugAsync(title);

        var article = new Article
        {
            Title = title,
            Slug = slug,
            Content = content,
            Excerpt = excerpt,
            AuthorId = authorId,
            CategoryId = categoryId,
            Status = publish ? ArticleStatus.Published : ArticleStatus.Draft,
            PublishedAt = publish ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Artykuł został dodany pomyślnie!";
        return RedirectToAction(nameof(Details), new { slug = article.Slug });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Edit(int id)
    {
        var article = await _context.Articles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article == null)
            return NotFound();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!User.IsInRole("Admin") && article.AuthorId.ToString() != userId)
            return Forbid();

        ViewBag.Categories = await _context.ArticleCategories.OrderBy(c => c.Name).ToListAsync();
        return View(article);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string title, string content, string? excerpt, int categoryId, bool publish)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null)
            return NotFound();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!User.IsInRole("Admin") && article.AuthorId.ToString() != userId)
            return Forbid();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            ModelState.AddModelError("", "Tytuł i treść są wymagane");
            ViewBag.Categories = await _context.ArticleCategories.OrderBy(c => c.Name).ToListAsync();
            return View(article);
        }

        var category = await _context.ArticleCategories.FindAsync(categoryId);
        if (category == null)
        {
            ModelState.AddModelError("", "Wybrana kategoria nie istnieje");
            ViewBag.Categories = await _context.ArticleCategories.OrderBy(c => c.Name).ToListAsync();
            return View(article);
        }

        var oldTitle = article.Title;
        article.Title = title;
        article.Content = content;
        article.Excerpt = excerpt;
        article.CategoryId = categoryId;
        article.Status = publish ? ArticleStatus.Published : ArticleStatus.Draft;

        if (publish && !article.PublishedAt.HasValue)
            article.PublishedAt = DateTime.UtcNow;

        // Regenerate slug only if title changed
        if (!string.Equals(oldTitle, title, StringComparison.Ordinal))
        {
            article.Slug = await GenerateUniqueSlugAsync(title, article.Id);
        }

        article.UpdatedAt = DateTime.UtcNow;

        _context.Articles.Update(article);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Artykuł został zaktualizowany!";
        return RedirectToAction(nameof(Details), new { slug = article.Slug });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await _context.Articles.FindAsync(id);
        if (article == null)
            return NotFound();

        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Artykuł został usunięty!";
        return RedirectToAction(nameof(Index));
    }
}
