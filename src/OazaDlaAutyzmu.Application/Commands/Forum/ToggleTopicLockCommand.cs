using MediatR;

namespace OazaDlaAutyzmu.Application.Commands.Forum;

public record ToggleTopicLockCommand : IRequest<bool>
{
    public int TopicId { get; init; }
}
