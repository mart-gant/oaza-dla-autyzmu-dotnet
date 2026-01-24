using MediatR;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Application.DTOs;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Queries.Reviews;

public class GetPendingReviewsQueryHandler : IRequestHandler<GetPendingReviewsQuery, List<ReviewDto>>
{
    private readonly ApplicationDbContext _context;

    public GetPendingReviewsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReviewDto>> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Facility)
            .Where(r => !r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                FacilityId = r.FacilityId,
                FacilityName = r.Facility.Name,
                UserId = r.UserId,
                UserName = r.User.FirstName + " " + r.User.LastName,
                Rating = r.Rating,
                Comment = r.Comment,
                IsApproved = r.IsApproved,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
