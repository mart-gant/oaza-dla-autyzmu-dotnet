using MediatR;

namespace OazaDlaAutyzmu.Application.Commands.Forum;

public record ToggleTopicPinCommand : IRequest<bool>
{
    public int TopicId { get; init; }
}
