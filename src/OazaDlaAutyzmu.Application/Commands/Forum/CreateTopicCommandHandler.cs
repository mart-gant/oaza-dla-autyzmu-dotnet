using MediatR;
using OazaDlaAutyzmu.Domain.Entities;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Commands.Forum;

public class CreateTopicCommandHandler : IRequestHandler<CreateTopicCommand, int>
{
    private readonly ApplicationDbContext _context;

    public CreateTopicCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = new ForumTopic
        {
            CategoryId = request.CategoryId,
            AuthorId = request.UserId,
            Title = request.Title,
            Slug = request.Title.ToLower().Replace(" ", "-"),
            IsLocked = false,
            IsPinned = false,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.ForumTopics.Add(topic);
        await _context.SaveChangesAsync(cancellationToken);

        // Create first post
        var post = new ForumPost
        {
            TopicId = topic.Id,
            AuthorId = request.UserId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.ForumPosts.Add(post);
        await _context.SaveChangesAsync(cancellationToken);

        return topic.Id;
    }
}
