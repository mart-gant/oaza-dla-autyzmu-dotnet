using MediatR;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Application.ViewModels;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Application.Queries.Admin;

public class GetDashboardStatisticsQueryHandler : IRequestHandler<GetDashboardStatisticsQuery, DashboardStatisticsViewModel>
{
    private readonly ApplicationDbContext _context;

    public GetDashboardStatisticsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatisticsViewModel> Handle(GetDashboardStatisticsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddMonths(-1);

        var stats = new DashboardStatisticsViewModel
        {
            TotalUsers = await _context.Users.CountAsync(cancellationToken),
            TotalFacilities = await _context.Facilities.CountAsync(cancellationToken),
            TotalReviews = await _context.Reviews.CountAsync(cancellationToken),
            PendingReviews = await _context.Reviews.CountAsync(r => !r.IsApproved, cancellationToken),
            TotalForumTopics = await _context.ForumTopics.CountAsync(cancellationToken),
            TotalForumPosts = await _context.ForumPosts.CountAsync(cancellationToken),
            NewUsersToday = await _context.Users.CountAsync(u => u.CreatedAt.Date == today, cancellationToken),
            NewUsersThisWeek = await _context.Users.CountAsync(u => u.CreatedAt >= weekAgo, cancellationToken),
            NewUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= monthAgo, cancellationToken)
        };

        // Recent activities from audit log
        stats.RecentActivities = await _context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .Select(a => new RecentActivityDto
            {
                Action = a.Action,
                UserEmail = a.UserEmail ?? "System",
                Timestamp = a.Timestamp,
                EntityType = a.EntityType
            })
            .ToListAsync(cancellationToken);

        // Top rated facilities
        stats.TopRatedFacilities = await _context.Facilities
            .Where(f => f.Reviews.Any(r => r.IsApproved))
            .Select(f => new TopFacilityDto
            {
                Id = f.Id,
                Name = f.Name,
                AverageRating = f.Reviews.Where(r => r.IsApproved).Average(r => (double)r.Rating),
                ReviewCount = f.Reviews.Count(r => r.IsApproved)
            })
            .OrderByDescending(f => f.AverageRating)
            .Take(5)
            .ToListAsync(cancellationToken);

        return stats;
    }
}
