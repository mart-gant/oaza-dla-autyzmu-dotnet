namespace OazaDlaAutyzmu.Domain.Entities;

public class ContactMessage : BaseEntity
{
    public int FacilityId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Facility Facility { get; set; } = null!;
}
