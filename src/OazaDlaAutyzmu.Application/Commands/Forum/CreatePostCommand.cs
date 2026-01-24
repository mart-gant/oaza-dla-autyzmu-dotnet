using MediatR;

namespace OazaDlaAutyzmu.Application.Commands.Forum;

public record CreatePostCommand : IRequest<int>
{
    public int TopicId { get; init; }
    public int UserId { get; init; }
    public string Content { get; init; } = string.Empty;
}
