using MediatR;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Application.DTOs;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Queries.Forum;

public class GetTopicByIdQueryHandler : IRequestHandler<GetTopicByIdQuery, ForumTopicDto?>
{
    private readonly ApplicationDbContext _context;

    public GetTopicByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ForumTopicDto?> Handle(GetTopicByIdQuery request, CancellationToken cancellationToken)
    {
        var topic = await _context.ForumTopics
            .Include(t => t.Author)
            .Include(t => t.Category)
            .Include(t => t.Posts)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (topic == null)
            return null;

        // Increment view count
        topic.ViewCount++;
        await _context.SaveChangesAsync(cancellationToken);

        return new ForumTopicDto
        {
            Id = topic.Id,
            CategoryId = topic.CategoryId,
            CategoryName = topic.Category.Name,
            Title = topic.Title,
            UserId = topic.AuthorId,
            UserName = topic.Author.UserName ?? "Anonim",
            IsLocked = topic.IsLocked,
            IsPinned = topic.IsPinned,
            ViewCount = topic.ViewCount,
            PostCount = topic.Posts.Count,
            CreatedAt = topic.CreatedAt
        };
    }
}
