using MediatR;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Application.DTOs;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Queries.Forum;

public class GetPostsByTopicQueryHandler : IRequestHandler<GetPostsByTopicQuery, List<ForumPostDto>>
{
    private readonly ApplicationDbContext _context;

    public GetPostsByTopicQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ForumPostDto>> Handle(GetPostsByTopicQuery request, CancellationToken cancellationToken)
    {
        var posts = await _context.ForumPosts
            .Include(p => p.Author)
            .Include(p => p.Topic)
            .Where(p => p.TopicId == request.TopicId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return posts
            .Select(p => new ForumPostDto
            {
                Id = p.Id,
                TopicId = p.TopicId,
                TopicTitle = p.Topic?.Title ?? "Unknown Topic",
                UserId = p.AuthorId,
                UserName = p.Author?.UserName ?? "Anonim",
                Content = p.Content,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToList();
    }
}
