using MediatR;
using OazaDlaAutyzmu.Application.DTOs;

namespace OazaDlaAutyzmu.Application.Queries.Reviews;

public record GetPendingReviewsQuery : IRequest<List<ReviewDto>>
{
}
