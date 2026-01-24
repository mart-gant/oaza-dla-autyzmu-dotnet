namespace OazaDlaAutyzmu.Domain.Entities;

public class Event : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? OrganizerName { get; set; }
    public string? OrganizerEmail { get; set; }
    public string? OrganizerPhone { get; set; }
    public string? Website { get; set; }
    public bool IsApproved { get; set; } = false;
    public int CreatedByUserId { get; set; }
    
    // Navigation properties
    public ApplicationUser CreatedBy { get; set; } = null!;
}
