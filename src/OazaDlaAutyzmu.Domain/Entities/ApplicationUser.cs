using Microsoft.AspNetCore.Identity;

namespace OazaDlaAutyzmu.Domain.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SuspendedAt { get; set; }
    public string? SuspensionReason { get; set; }
    
    // Navigation properties
    public ICollection<ForumTopic> ForumTopics { get; set; } = new List<ForumTopic>();
    public ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
    
    public bool IsAdmin() => Role == UserRole.Admin;
    public bool IsModerator() => Role == UserRole.Moderator || Role == UserRole.Admin;
}

public enum UserRole
{
    User,
    Moderator,
    Admin
}
