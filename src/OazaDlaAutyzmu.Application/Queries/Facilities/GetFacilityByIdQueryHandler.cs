using MediatR;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Application.DTOs;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Queries.Facilities;

public class GetFacilityByIdQueryHandler : IRequestHandler<GetFacilityByIdQuery, FacilityDto?>
{
    private readonly ApplicationDbContext _context;

    public GetFacilityByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FacilityDto?> Handle(GetFacilityByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Facilities
            .Include(f => f.VerifiedBy)
            .Include(f => f.Reviews)
            .Where(f => f.Id == request.Id)
            .Select(f => new FacilityDto
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                Address = f.Address,
                City = f.City,
                PostalCode = f.PostalCode,
                PhoneNumber = f.PhoneNumber,
                Email = f.Email,
                Website = f.Website,
                Type = f.Type.ToString(),
                Latitude = f.Latitude,
                Longitude = f.Longitude,
                Source = f.Source,
                VerificationStatus = f.VerificationStatus.ToString(),
                VerifiedByName = f.VerifiedBy != null ? f.VerifiedBy.UserName : null,
                VerifiedAt = f.VerifiedAt,
                VerificationNotes = f.VerificationNotes,
                CreatedAt = f.CreatedAt,
                ReviewCount = f.Reviews.Count,
                AverageRating = f.Reviews.Any() ? f.Reviews.Average(r => r.Rating) : null
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
