namespace OazaDlaAutyzmu.Application.DTOs;

public class FacilityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Source { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public string? VerifiedByName { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ReviewCount { get; set; }
    public double? AverageRating { get; set; }
}
