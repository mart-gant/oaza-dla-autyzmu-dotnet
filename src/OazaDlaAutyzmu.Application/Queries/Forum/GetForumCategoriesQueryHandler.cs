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
                .Include(t => t.Posts)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            ForumTopicDto? latestTopicDto = null;
            if (latestTopic != null)
            {
                latestTopicDto = new ForumTopicDto
                {
                    Id = latestTopic.Id,
                    CategoryId = latestTopic.CategoryId,
                    CategoryName = category.Name,
                    Title = latestTopic.Title,
                    UserId = latestTopic.AuthorId,
                    UserName = latestTopic.Author?.UserName ?? "Anonim",
                    IsLocked = latestTopic.IsLocked,
                    IsPinned = latestTopic.IsPinned,
                    ViewCount = latestTopic.ViewCount,
                    PostCount = latestTopic.Posts?.Count ?? 0,
                    CreatedAt = latestTopic.CreatedAt
                };
            }

            result.Add(new ForumCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                TopicCount = topicCount,
                PostCount = postCount,
                LatestTopic = latestTopicDto
            });
        }

        return result;
    }
}
