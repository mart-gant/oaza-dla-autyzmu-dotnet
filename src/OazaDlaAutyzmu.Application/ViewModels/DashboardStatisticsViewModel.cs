namespace OazaDlaAutyzmu.Application.ViewModels;

public class DashboardStatisticsViewModel
{
    public int TotalUsers { get; set; }
    public int TotalFacilities { get; set; }
    public int TotalReviews { get; set; }
    public int PendingReviews { get; set; }
    public int TotalForumTopics { get; set; }
    public int TotalForumPosts { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    public List<TopFacilityDto> TopRatedFacilities { get; set; } = new();
}

public class RecentActivityDto
{
    public string Action { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string EntityType { get; set; } = string.Empty;
}

public class TopFacilityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
