namespace OazaDlaAutyzmu.Domain.Entities;

public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty; // ReviewApproved, ReviewRejected, TopicReply, Mention, System
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Url { get; set; } // Optional link to relevant entity
    public bool IsRead { get; set; } = false;
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ApplicationUser? User { get; set; }
}
