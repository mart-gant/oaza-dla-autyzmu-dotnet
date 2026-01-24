using MediatR;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Commands.Reviews;

public class ApproveReviewCommandHandler : IRequestHandler<ApproveReviewCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public ApproveReviewCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _context.Reviews.FindAsync(request.ReviewId);
        if (review == null)
            return false;

        review.IsApproved = true;
        review.ApprovedById = request.ModeratorId;
        review.ApprovedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
