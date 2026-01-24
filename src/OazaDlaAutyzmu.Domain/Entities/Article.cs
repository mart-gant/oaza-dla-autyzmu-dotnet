namespace OazaDlaAutyzmu.Domain.Entities;

public class Article : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? FeaturedImage { get; set; }
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; } = 0;
    
    // Navigation properties
    public ApplicationUser Author { get; set; } = null!;
    public ArticleCategory Category { get; set; } = null!;
    public ICollection<ArticleTag> Tags { get; set; } = new List<ArticleTag>();
}

public class ArticleCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Navigation properties
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}

public class ArticleTag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}

public enum ArticleStatus
{
    Draft,
    Published,
    Archived
}
