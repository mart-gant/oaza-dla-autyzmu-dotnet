using MediatR;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Commands.Facilities;

public class DeleteFacilityCommandHandler : IRequestHandler<DeleteFacilityCommand, Unit>
{
    private readonly ApplicationDbContext _context;

    public DeleteFacilityCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteFacilityCommand request, CancellationToken cancellationToken)
    {
        var facility = await _context.Facilities
            .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

        if (facility == null)
            throw new KeyNotFoundException($"Facility with ID {request.Id} not found");

        _context.Facilities.Remove(facility);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
