using Core;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Config
{
    public class XeroAppDbContext : IdentityDbContext<User, Role, long>
    {
        public XeroAppDbContext(DbContextOptions options) : base(options)
        {
        }

        public XeroAppDbContext()
        {
        }

        public DbSet<XeroClientApp> ClientApps { get; set; }
        public DbSet<XeroSessionClientId> SessionClientIds { get; set; }
        public DbSet<XeroToken> XeroTokens { get; set; }
        public DbSet<XeroTenant> XeroTenant { get; set; }
        public DbSet<XeroClientDataModel> XeroClient { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlan { get; set; }
        public DbSet<XeroUserTenantsSubscription> XeroUserTenantsSubscription { get; set; }
        public DbSet<FinancialInstitutionCode> FinancialInstitutionCodes { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserTrack> UserTrack { get; set; }
        public DbSet<TransactionTrack> TransactionTrack { get; set; }
        public DbSet<ErrorTrack> ErrorTrack { get; set; }
        public virtual DbSet<Idea> Ideas { get; set; } = null!;
        public virtual DbSet<IdeaImage> IdeaImages { get; set; } = null!;
        public virtual DbSet<IdeaVoteDetail> IdeaVoteDetails { get; set; } = null!;
        public virtual DbSet<ReportBug> ReportBug { get; set; }

        public virtual DbSet<XeroApiLog> XeroApiLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new XeroClientAppConfig());
            builder.ApplyConfiguration(new XeroSessionClientIdConfig());
            builder.ApplyConfiguration(new XeroTokenConfig());
            builder.ApplyConfiguration(new FinancialInstitutionCodeConfig());
            builder.ApplyConfiguration(new UsersConfig());
            builder.ApplyConfiguration(new UserTracksConfig());
            builder.ApplyConfiguration(new ReportBugConfig());
            builder.ApplyConfiguration(new IdeaConfig());
            builder.ApplyConfiguration(new IdeaImageConfig());
            builder.ApplyConfiguration(new IdeaVoteDetailConfig());
            builder.ApplyConfiguration(new TransactionTrackConfig());
            builder.ApplyConfiguration(new ErrorTrackConfig());
            builder.ApplyConfiguration(new XeroApiLogConfig());

        }

    }
}
