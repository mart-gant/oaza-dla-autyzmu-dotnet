namespace OazaDlaAutyzmu.Domain.Entities;

public class Review : BaseEntity
{
    public int FacilityId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string? Comment { get; set; }
    public bool IsApproved { get; set; } = false;
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Navigation properties
    public Facility Facility { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public ApplicationUser? ApprovedBy { get; set; }
}
