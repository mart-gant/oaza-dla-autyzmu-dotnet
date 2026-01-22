namespace OazaDlaAutyzmu.Domain.Entities;

public class Facility : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public FacilityType Type { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    // Verification fields (from Laravel migration)
    public string? Source { get; set; }
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Unverified;
    public int? VerifiedById { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationNotes { get; set; }
    
    // Navigation properties
    public ApplicationUser? VerifiedBy { get; set; }
    
    // Helper methods
    public bool IsVerified() => VerificationStatus == VerificationStatus.Verified || VerificationStatus == VerificationStatus.Certified;
    
    public bool IsCertified() => VerificationStatus == VerificationStatus.Certified;
    
    public string GetVerificationBadge()
    {
        return VerificationStatus switch
        {
            VerificationStatus.Certified => "<span class=\"px-3 py-1 bg-green-100 text-green-800 rounded-full text-sm\">✓ Certyfikat</span>",
            VerificationStatus.Verified => "<span class=\"px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm\">✓ Zweryfikowana</span>",
            VerificationStatus.Flagged => "<span class=\"px-3 py-1 bg-yellow-100 text-yellow-800 rounded-full text-sm\">⚠ Wymaga sprawdzenia</span>",
            _ => "<span class=\"px-3 py-1 bg-gray-100 text-gray-800 rounded-full text-sm\">Niezweryfikowana</span>"
        };
    }
}

public enum VerificationStatus
{
    Unverified,
    Verified,
    Certified,
    Flagged
}

public enum FacilityType
{
    Therapy,
    School,
    SupportCenter,
    Clinic,
    Other
}
