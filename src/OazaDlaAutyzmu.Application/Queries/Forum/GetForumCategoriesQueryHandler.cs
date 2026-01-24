using MediatR;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Application.DTOs;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Queries.Forum;

public class GetForumCategoriesQueryHandler : IRequestHandler<GetForumCategoriesQuery, List<ForumCategoryDto>>
{
    private readonly ApplicationDbContext _context;

    public GetForumCategoriesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ForumCategoryDto>> Handle(GetForumCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.ForumCategories
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

        var result = new List<ForumCategoryDto>();

        foreach (var category in categories)
        {
            var topics = await _context.ForumTopics
                .Where(t => t.CategoryId == category.Id)
                .ToListAsync(cancellationToken);

            var topicCount = topics.Count;
            var postCount = await _context.ForumPosts
                .Where(p => topics.Select(t => t.Id).Contains(p.TopicId))
                .CountAsync(cancellationToken);

            var latestTopic = await _context.ForumTopics
                .Where(t => t.CategoryId == category.Id)
                .Include(t => t.Author)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new ForumTopicDto
                {
                    Id = t.Id,
                    CategoryId = t.CategoryId,
                    CategoryName = category.Name,
                    Title = t.Title,
                    UserId = t.AuthorId,
                    UserName = t.Author.UserName ?? "Anonim",
                    IsLocked = t.IsLocked,
                    IsPinned = t.IsPinned,
                    ViewCount = t.ViewCount,
                    PostCount = t.Posts.Count,
                    CreatedAt = t.CreatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            result.Add(new ForumCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                TopicCount = topicCount,
                PostCount = postCount,
                LatestTopic = latestTopic
            });
        }

        return result;
    }
}
