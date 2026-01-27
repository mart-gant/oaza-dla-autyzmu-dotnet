using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Domain.Entities;

namespace OazaDlaAutyzmu.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Facility> Facilities { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<ArticleCategory> ArticleCategories { get; set; }
    public DbSet<ArticleTag> ArticleTags { get; set; }
    public DbSet<ForumCategory> ForumCategories { get; set; }
    public DbSet<ForumTopic> ForumTopics { get; set; }
    public DbSet<ForumPost> ForumPosts { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<FacilityImage> FacilityImages { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Facility configuration
        modelBuilder.Entity<Facility>(entity =>
        {
            entity.ToTable("facilities");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.City)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.PostalCode)
                .HasMaxLength(20);
            
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50);
            
            entity.Property(e => e.Email)
                .HasMaxLength(255);
            
            entity.Property(e => e.Website)
                .HasMaxLength(500);
            
            entity.Property(e => e.Latitude)
                .HasPrecision(10, 8);
            
            entity.Property(e => e.Longitude)
                .HasPrecision(11, 8);
            
            entity.Property(e => e.Source)
                .HasMaxLength(255);
            
            entity.Property(e => e.VerificationNotes)
                .HasColumnType("text");
            
            // Relationship with User (VerifiedBy)
            entity.HasOne(e => e.VerifiedBy)
                .WithMany()
                .HasForeignKey(e => e.VerifiedById)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Indexes for performance
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.VerificationStatus);
            entity.HasIndex(e => e.CreatedAt);
        });

        // ApplicationUser configuration
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users");
            
            entity.Property(e => e.FirstName)
                .HasMaxLength(100);
            
            entity.Property(e => e.LastName)
                .HasMaxLength(100);
            
            entity.Property(e => e.SuspensionReason)
                .HasMaxLength(500);
        });

        // Rename Identity tables to match Laravel naming convention
        modelBuilder.Entity<IdentityRole<int>>().ToTable("roles");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("user_roles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("user_claims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("user_logins");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("user_tokens");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("role_claims");

        // Review configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("reviews");
            
            entity.Property(e => e.Rating)
                .IsRequired();
            
            entity.Property(e => e.Comment)
                .HasColumnType("text");
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Facility)
                .WithMany(f => f.Reviews)
                .HasForeignKey(e => e.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ApprovedBy)
                .WithMany()
                .HasForeignKey(e => e.ApprovedById)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Performance indexes
            entity.HasIndex(e => e.FacilityId);
            entity.HasIndex(e => e.IsApproved);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.FacilityId, e.IsApproved });
        });

        // Article configuration
        modelBuilder.Entity<Article>(entity =>
        {
            entity.ToTable("articles");
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("text");
            
            entity.HasOne(e => e.Author)
                .WithMany(u => u.Articles)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Articles)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PublishedAt);
        });

        // ArticleCategory configuration
        modelBuilder.Entity<ArticleCategory>(entity =>
        {
            entity.ToTable("article_categories");
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // ArticleTag configuration
        modelBuilder.Entity<ArticleTag>(entity =>
        {
            entity.ToTable("article_tags");
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // ForumCategory configuration
        modelBuilder.Entity<ForumCategory>(entity =>
        {
            entity.ToTable("forum_categories");
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.SortOrder);
        });

        // ForumTopic configuration
        modelBuilder.Entity<ForumTopic>(entity =>
        {
            entity.ToTable("forum_topics");
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.Slug)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Topics)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Author)
                .WithMany(u => u.ForumTopics)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.LastPostUser)
                .WithMany()
                .HasForeignKey(e => e.LastPostUserId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.IsPinned);
            entity.HasIndex(e => e.LastPostAt);
        });

        // ForumPost configuration
        modelBuilder.Entity<ForumPost>(entity =>
        {
            entity.ToTable("forum_posts");
            
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("text");
            
            entity.HasOne(e => e.Topic)
                .WithMany(t => t.Posts)
                .HasForeignKey(e => e.TopicId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Author)
                .WithMany(u => u.ForumPosts)
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ApprovedBy)
                .WithMany()
                .HasForeignKey(e => e.ApprovedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Event configuration
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.Location)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.HasOne(e => e.CreatedBy)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.IsApproved);
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.UserId)
                .IsRequired();
            
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.Message)
                .IsRequired();
            
            entity.Property(e => e.Url)
                .HasMaxLength(500);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Seed data (using static dates to avoid model changes warning)
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        modelBuilder.Entity<ArticleCategory>().HasData(
            new ArticleCategory { Id = 1, Name = "Podstawy", Slug = "podstawy", CreatedAt = seedDate },
            new ArticleCategory { Id = 2, Name = "Diagnoza", Slug = "diagnoza", CreatedAt = seedDate },
            new ArticleCategory { Id = 3, Name = "Terapie", Slug = "terapie", CreatedAt = seedDate },
            new ArticleCategory { Id = 4, Name = "Edukacja", Slug = "edukacja", CreatedAt = seedDate },
            new ArticleCategory { Id = 5, Name = "Wsparcie rodzin", Slug = "wsparcie-rodzin", CreatedAt = seedDate }
        );

        modelBuilder.Entity<ForumCategory>().HasData(
            new ForumCategory { Id = 1, Name = "Ogólne", Slug = "ogolne", Description = "Dyskusje ogólne o autyzmie", SortOrder = 1, CreatedAt = seedDate },
            new ForumCategory { Id = 2, Name = "Diagnoza", Slug = "diagnoza", Description = "Pytania o diagnozę i specjalistów", SortOrder = 2, CreatedAt = seedDate },
            new ForumCategory { Id = 3, Name = "Terapie", Slug = "terapie", Description = "Doświadczenia z terapiami", SortOrder = 3, CreatedAt = seedDate },
            new ForumCategory { Id = 4, Name = "Szkoła i edukacja", Slug = "szkola-edukacja", Description = "Edukacja dzieci z autyzmem", SortOrder = 4, CreatedAt = seedDate },
            new ForumCategory { Id = 5, Name = "Wsparcie", Slug = "wsparcie", Description = "Wsparcie dla rodziców i opiekunów", SortOrder = 5, CreatedAt = seedDate }
        );

        // FacilityImage configuration
        modelBuilder.Entity<FacilityImage>(entity =>
        {
            entity.ToTable("facility_images");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.Caption)
                .HasMaxLength(200);
            
            entity.HasOne(e => e.Facility)
                .WithMany()
                .HasForeignKey(e => e.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.FacilityId);
            entity.HasIndex(e => new { e.FacilityId, e.IsMain });
            entity.HasIndex(e => new { e.FacilityId, e.DisplayOrder });
        });

        // ContactMessage configuration
        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.ToTable("contact_messages");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.SenderName)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.SenderEmail)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.Subject)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(2000);
            
            entity.HasOne(e => e.Facility)
                .WithMany()
                .HasForeignKey(e => e.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.FacilityId);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.SentAt);
        });
    }
}
