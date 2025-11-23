using AnimalRescueSystem.Constants;
using AnimalRescueSystem.Entities.RequestRescues;
using Microsoft.EntityFrameworkCore;
using Pawchums.Entities.EmailVerification;
using Pawchums.Entities.RequestRescues;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Pawchums.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class PawchumsDbContext :
    AbpDbContext<PawchumsDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    // Custom Entities
    public DbSet<RequestRescue> RequestRescues { get; set; }
    public DbSet<RescueInitiation> RescueInitiations { get; set; }
    public DbSet<RescueCompletion> RescueCompletions { get; set; }
    public DbSet<RescuerNotification> RescuerNotifications { get; set; }
    public DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }

    public PawchumsDbContext(DbContextOptions<PawchumsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        // RequestRescue Configuration
        builder.Entity<RequestRescue>(b =>
        {
            b.ToTable(PawchumsConsts.DbTablePrefix + "RequestRescues", PawchumsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(t => t.Id);

            // Request Information
            b.Property(t => t.Title).HasMaxLength(RequestRescueConsts.MaxLength.Title).IsRequired();
            b.Property(t => t.Location).HasMaxLength(RequestRescueConsts.MaxLength.Location).IsRequired();
            b.Property(t => t.Description).HasMaxLength(RequestRescueConsts.MaxLength.Description).IsRequired();
            // Picture stores base64 image - no max length, uses TEXT for PostgreSQL
            b.Property(t => t.Picture).HasColumnType("text").IsRequired(false);
            b.Property(t => t.ContactNo).HasMaxLength(RequestRescueConsts.MaxLength.ContactNo).IsRequired();
            b.Property(t => t.ContactName).HasMaxLength(RequestRescueConsts.MaxLength.ContactName).IsRequired(false);
            b.Property(t => t.RequestDate).IsRequired();

            // Status
            b.Property(t => t.Status).HasMaxLength(RequestRescueConsts.MaxLength.Status).IsRequired();

            // Active Status
            b.Property(t => t.IsActive).IsRequired();

            // Relationships
            b.HasMany(t => t.RescueInitiations)
                .WithOne(ri => ri.RequestRescue)
                .HasForeignKey(ri => ri.RequestRescueId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(t => t.RescueCompletion)
                .WithOne(rc => rc.RequestRescue)
                .HasForeignKey<RescueCompletion>(rc => rc.RequestRescueId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            b.HasIndex(t => t.Status);
            b.HasIndex(t => t.RequestDate);
            b.HasIndex(t => t.IsActive);
        });

        // RescueInitiation Configuration
        builder.Entity<RescueInitiation>(b =>
        {
            b.ToTable(PawchumsConsts.DbTablePrefix + "RescueInitiations", PawchumsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(t => t.Id);

            // Properties
            b.Property(t => t.RequestRescueId).IsRequired();
            b.Property(t => t.RescuerId).IsRequired();
            b.Property(t => t.InitiatedDate).IsRequired();
            b.Property(t => t.Notes).HasMaxLength(RescueInitiationConsts.MaxLength.Notes).IsRequired(false);
            b.Property(t => t.Status).HasMaxLength(RescueInitiationConsts.MaxLength.Status).IsRequired();
            b.Property(t => t.IsSelected).IsRequired();
            b.Property(t => t.AcceptedDate).IsRequired(false);
            b.Property(t => t.AcceptedByUserId).IsRequired(false);

            // Foreign Key to ABP Users
            b.HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(t => t.RescuerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            b.HasIndex(t => t.RequestRescueId);
            b.HasIndex(t => t.RescuerId);
            b.HasIndex(t => t.Status);
            b.HasIndex(t => t.IsSelected);
            b.HasIndex(t => new { t.RequestRescueId, t.RescuerId }).IsUnique(); // One initiation per rescuer per request
        });

        // RescueCompletion Configuration
        builder.Entity<RescueCompletion>(b =>
        {
            b.ToTable(PawchumsConsts.DbTablePrefix + "RescueCompletions", PawchumsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(t => t.Id);

            // Properties
            b.Property(t => t.RequestRescueId).IsRequired();
            // CompletionProofPicture stores base64 image - no max length, uses TEXT for PostgreSQL
            b.Property(t => t.CompletionProofPicture).HasColumnType("text").IsRequired(false);
            b.Property(t => t.CompletionDate).IsRequired();
            b.Property(t => t.CompletionDescription).HasMaxLength(RescueCompletionConsts.MaxLength.CompletionDescription).IsRequired(false);
            b.Property(t => t.CompletedByRescuerId).IsRequired();
            b.Property(t => t.IsVerified).IsRequired();
            b.Property(t => t.VerifiedByUserId).IsRequired(false);
            b.Property(t => t.VerifiedDate).IsRequired(false);
            b.Property(t => t.VerificationNotes).HasMaxLength(RescueCompletionConsts.MaxLength.VerificationNotes).IsRequired(false);

            // Foreign Key to ABP Users
            b.HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(t => t.CompletedByRescuerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            b.HasIndex(t => t.RequestRescueId).IsUnique(); // One-to-one
            b.HasIndex(t => t.CompletedByRescuerId);
            b.HasIndex(t => t.IsVerified);
        });

        // RescuerNotification Configuration
        builder.Entity<RescuerNotification>(b =>
        {
            b.ToTable(PawchumsConsts.DbTablePrefix + "RescuerNotifications", PawchumsConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(t => t.Id);

            // Properties
            b.Property(t => t.RequestRescueId).IsRequired();
            b.Property(t => t.RescuerId).IsRequired();
            b.Property(t => t.SentDate).IsRequired();
            b.Property(t => t.IsRead).IsRequired();
            b.Property(t => t.ReadDate).IsRequired(false);
            b.Property(t => t.NotificationType).HasMaxLength(RescuerNotificationConsts.MaxLength.NotificationType).IsRequired();
            b.Property(t => t.Message).HasMaxLength(RescuerNotificationConsts.MaxLength.Message).IsRequired(false);

            // Relationships
            b.HasOne(t => t.RequestRescue)
                .WithMany()
                .HasForeignKey(t => t.RequestRescueId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(t => t.RescuerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            b.HasIndex(t => t.RequestRescueId);
            b.HasIndex(t => t.RescuerId);
            b.HasIndex(t => t.IsRead);
            b.HasIndex(t => new { t.RescuerId, t.IsRead }); // For fetching unread notifications
        });
    }
}