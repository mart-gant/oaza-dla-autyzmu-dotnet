using MediatR;
using OazaDlaAutyzmu.Domain.Entities;

namespace OazaDlaAutyzmu.Application.Commands.Facilities;

public record CreateFacilityCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string? PostalCode { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public string? Website { get; init; }
    public FacilityType Type { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? Source { get; init; }
}
