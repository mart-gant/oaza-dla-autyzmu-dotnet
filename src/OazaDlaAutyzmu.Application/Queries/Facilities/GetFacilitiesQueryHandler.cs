using MediatR;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Application.Common;
using OazaDlaAutyzmu.Application.DTOs;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Queries.Facilities;

public class GetFacilitiesQueryHandler : IRequestHandler<GetFacilitiesQuery, PagedResult<FacilityDto>>
{
    private readonly ApplicationDbContext _context;

    public GetFacilitiesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<FacilityDto>> Handle(GetFacilitiesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Facilities
            .Include(f => f.VerifiedBy)
            .Include(f => f.Reviews)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.City))
            query = query.Where(f => f.City.Contains(request.City));

        if (request.Type.HasValue)
            query = query.Where(f => f.Type == request.Type);

        if (request.Status.HasValue)
            query = query.Where(f => f.VerificationStatus == request.Status);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(f => 
                f.Name.Contains(request.SearchTerm) || 
                f.Description!.Contains(request.SearchTerm) ||
                f.Address.Contains(request.SearchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
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
            .OrderByDescending(f => f.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<FacilityDto>(items, totalCount, request.PageNumber, request.PageSize);
    }
}
