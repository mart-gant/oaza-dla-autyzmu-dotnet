using MediatR;

namespace OazaDlaAutyzmu.Application.Commands.Reviews;

public record CreateReviewCommand : IRequest<int>
{
    public int FacilityId { get; init; }
    public int UserId { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
}
