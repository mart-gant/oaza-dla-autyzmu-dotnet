namespace OazaDlaAutyzmu.Domain.Entities;

public class FacilityImage : BaseEntity
{
    public int FacilityId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsMain { get; set; } = false;

    // Navigation property
    public Facility Facility { get; set; } = null!;
}
