using MediatR;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Commands.Reviews;

public class RejectReviewCommandHandler : IRequestHandler<RejectReviewCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public RejectReviewCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _context.Reviews.FindAsync(request.ReviewId);
        if (review == null)
            return false;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
