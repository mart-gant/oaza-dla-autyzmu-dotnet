using MediatR;

namespace OazaDlaAutyzmu.Application.Commands.Forum;

public record CreateTopicCommand : IRequest<int>
{
    public int CategoryId { get; init; }
    public int UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}
