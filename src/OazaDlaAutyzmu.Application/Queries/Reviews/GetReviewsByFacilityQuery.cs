using MediatR;
using OazaDlaAutyzmu.Application.DTOs;

namespace OazaDlaAutyzmu.Application.Queries.Reviews;

public record GetReviewsByFacilityQuery : IRequest<List<ReviewDto>>
{
    public int FacilityId { get; init; }
    public bool OnlyApproved { get; init; } = true;
}
