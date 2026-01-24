using MediatR;

namespace OazaDlaAutyzmu.Application.Commands.Reviews;

public record ApproveReviewCommand : IRequest<bool>
{
    public int ReviewId { get; init; }
    public int ModeratorId { get; init; }
}
