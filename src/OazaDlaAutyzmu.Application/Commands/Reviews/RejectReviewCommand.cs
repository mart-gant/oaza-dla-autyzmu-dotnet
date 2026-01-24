using MediatR;

namespace OazaDlaAutyzmu.Application.Commands.Reviews;

public record RejectReviewCommand : IRequest<bool>
{
    public int ReviewId { get; init; }
}
