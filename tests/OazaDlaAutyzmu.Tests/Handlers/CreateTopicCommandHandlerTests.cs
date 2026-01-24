using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Application.Commands.Forum;
using OazaDlaAutyzmu.Domain.Entities;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Tests.Handlers;

public class CreateTopicCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CreateTopicCommandHandler _handler;

    public CreateTopicCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new CreateTopicCommandHandler(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var category = new ForumCategory
        {
            Name = "Ogólne",
            Description = "Ogólne dyskusje",
            Slug = "ogolne"
        };

        _context.ForumCategories.Add(category);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_Should_Create_Topic_Successfully()
    {
        // Arrange
        var category = await _context.ForumCategories.FirstAsync();
        var command = new CreateTopicCommand
        {
            CategoryId = category.Id,
            UserId = 1,
            Title = "Test Topic Title",
            Content = "This is the initial post content."
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);

        var topic = await _context.ForumTopics
            .Include(t => t.Posts)
            .FirstOrDefaultAsync(t => t.Id == result);

        topic.Should().NotBeNull();
        topic!.Title.Should().Be("Test Topic Title");
        topic.CategoryId.Should().Be(category.Id);
        topic.IsPinned.Should().BeFalse();
        topic.IsLocked.Should().BeFalse();
        
        // Should create initial post
        topic.Posts.Should().HaveCount(1);
        topic.Posts.First().Content.Should().Be("This is the initial post content.");
    }

    [Fact]
    public async Task Handle_Should_Create_Corresponding_Initial_Post()
    {
        // Arrange
        var category = await _context.ForumCategories.FirstAsync();

        var command = new CreateTopicCommand
        {
            CategoryId = category.Id,
            UserId = 1,
            Title = "Another Topic",
            Content = "Another initial post."
        };

        // Act
        var topicId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var posts = await _context.ForumPosts.Where(p => p.TopicId == topicId).ToListAsync();
        posts.Should().HaveCount(1);
        posts.First().Content.Should().Be("Another initial post.");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
