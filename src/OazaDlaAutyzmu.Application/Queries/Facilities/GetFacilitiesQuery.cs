using MediatR;
using OazaDlaAutyzmu.Application.Common;
using OazaDlaAutyzmu.Application.DTOs;
using OazaDlaAutyzmu.Domain.Entities;

namespace OazaDlaAutyzmu.Application.Queries.Facilities;

public record GetFacilitiesQuery : IRequest<PagedResult<FacilityDto>>
{
    public string? City { get; init; }
    public FacilityType? Type { get; init; }
    public VerificationStatus? Status { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}
