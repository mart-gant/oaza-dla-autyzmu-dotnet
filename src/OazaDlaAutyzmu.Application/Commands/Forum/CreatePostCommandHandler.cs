using MediatR;
using OazaDlaAutyzmu.Domain.Entities;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Commands.Forum;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, int>
{
    private readonly ApplicationDbContext _context;

    public CreatePostCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new ForumPost
        {
            TopicId = request.TopicId,
            AuthorId = request.UserId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.ForumPosts.Add(post);
        await _context.SaveChangesAsync(cancellationToken);

        return post.Id;
    }
}
