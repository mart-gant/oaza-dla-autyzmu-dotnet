namespace OazaDlaAutyzmu.Domain.Entities;

public class ForumCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    public ICollection<ForumTopic> Topics { get; set; } = new List<ForumTopic>();
}

public class ForumTopic : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int AuthorId { get; set; }
    public bool IsPinned { get; set; } = false;
    public bool IsLocked { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public int PostCount { get; set; } = 0;
    public DateTime? LastPostAt { get; set; }
    public int? LastPostUserId { get; set; }
    
    // Navigation properties
    public ForumCategory Category { get; set; } = null!;
    public ApplicationUser Author { get; set; } = null!;
    public ApplicationUser? LastPostUser { get; set; }
    public ICollection<ForumPost> Posts { get; set; } = new List<ForumPost>();
}

public class ForumPost : BaseEntity
{
    public int TopicId { get; set; }
    public int AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = true;
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public string? EditReason { get; set; }
    
    // Navigation properties
    public ForumTopic Topic { get; set; } = null!;
    public ApplicationUser Author { get; set; } = null!;
    public ApplicationUser? ApprovedBy { get; set; }
}
