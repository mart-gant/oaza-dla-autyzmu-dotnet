using MediatR;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Application.DTOs;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Queries.Reviews;

public class GetReviewsByFacilityQueryHandler : IRequestHandler<GetReviewsByFacilityQuery, List<ReviewDto>>
{
    private readonly ApplicationDbContext _context;

    public GetReviewsByFacilityQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReviewDto>> Handle(GetReviewsByFacilityQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Facility)
            .Include(r => r.ApprovedBy)
            .Where(r => r.FacilityId == request.FacilityId);

        if (request.OnlyApproved)
            query = query.Where(r => r.IsApproved);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                FacilityId = r.FacilityId,
                FacilityName = r.Facility.Name,
                UserId = r.UserId,
                UserName = r.User.UserName ?? "Anonim",
                Rating = r.Rating,
                Comment = r.Comment,
                IsApproved = r.IsApproved,
                ApprovedByName = r.ApprovedBy != null ? r.ApprovedBy.UserName : null,
                ApprovedAt = r.ApprovedAt,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
