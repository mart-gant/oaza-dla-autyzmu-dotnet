using MediatR;

namespace OazaDlaAutyzmu.Application.Commands.Facilities;

public record DeleteFacilityCommand : IRequest<Unit>
{
    public int Id { get; init; }
}
