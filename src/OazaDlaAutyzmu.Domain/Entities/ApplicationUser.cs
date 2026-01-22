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
    
    public bool IsAdmin() => Role == UserRole.Admin;
    public bool IsModerator() => Role == UserRole.Moderator || Role == UserRole.Admin;
}

public enum UserRole
{
    User,
    Moderator,
    Admin
}
