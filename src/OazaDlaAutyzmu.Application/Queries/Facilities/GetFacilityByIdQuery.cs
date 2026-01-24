using MediatR;
using OazaDlaAutyzmu.Application.DTOs;

namespace OazaDlaAutyzmu.Application.Queries.Facilities;

public record GetFacilityByIdQuery : IRequest<FacilityDto?>
{
    public int Id { get; init; }
}
