using MediatR;
using OazaDlaAutyzmu.Domain.Entities;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Commands.Facilities;

public class CreateFacilityCommandHandler : IRequestHandler<CreateFacilityCommand, int>
{
    private readonly ApplicationDbContext _context;

    public CreateFacilityCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateFacilityCommand request, CancellationToken cancellationToken)
    {
        var facility = new Facility
        {
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            City = request.City,
            PostalCode = request.PostalCode,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Website = request.Website,
            Type = request.Type,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Source = request.Source,
            CreatedAt = DateTime.UtcNow
        };

        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync(cancellationToken);

        return facility.Id;
    }
}
