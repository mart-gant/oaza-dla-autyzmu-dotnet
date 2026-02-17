using MediatR;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Application.DTOs;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Queries.Forum;

public class GetTopicsByCategoryQueryHandler : IRequestHandler<GetTopicsByCategoryQuery, List<ForumTopicDto>>
{
    private readonly ApplicationDbContext _context;

    public GetTopicsByCategoryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ForumTopicDto>> Handle(GetTopicsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var topics = await _context.ForumTopics
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Author)
            .Include(t => t.Posts)
            .Where(t => t.CategoryId == request.CategoryId)
            .ToListAsync(cancellationToken);

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
        });

        // Sort: pinned first, then by latest post or created date
        return topicDtos
            .OrderByDescending(t => t.IsPinned)
            .ThenByDescending(t => t.LatestPost?.CreatedAt ?? t.CreatedAt)
            .ToList();
    }
}
