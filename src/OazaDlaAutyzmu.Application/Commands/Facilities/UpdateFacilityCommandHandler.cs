using MediatR;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Commands.Facilities;

public class UpdateFacilityCommandHandler : IRequestHandler<UpdateFacilityCommand, Unit>
{
    private readonly ApplicationDbContext _context;

    public UpdateFacilityCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateFacilityCommand request, CancellationToken cancellationToken)
    {
        var facility = await _context.Facilities
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (facility == null)
            throw new KeyNotFoundException($"Facility with ID {request.Id} not found");

        facility.Name = request.Name;
        facility.Description = request.Description;
        facility.Address = request.Address;
        facility.City = request.City;
        facility.PostalCode = request.PostalCode;
        facility.PhoneNumber = request.PhoneNumber;
        facility.Email = request.Email;
        facility.Website = request.Website;
        facility.Type = request.Type;
        facility.Latitude = request.Latitude;
        facility.Longitude = request.Longitude;
        facility.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
