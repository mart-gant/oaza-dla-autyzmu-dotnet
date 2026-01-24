namespace OazaDlaAutyzmu.Application.DTOs;

public class ForumCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TopicCount { get; set; }
    public int PostCount { get; set; }
    public ForumTopicDto? LatestTopic { get; set; }
}

public class ForumTopicDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public bool IsPinned { get; set; }
    public int ViewCount { get; set; }
    public int PostCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public ForumPostDto? LatestPost { get; set; }
}

public class ForumPostDto
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string TopicTitle { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
