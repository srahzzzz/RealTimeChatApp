using Microsoft.EntityFrameworkCore;
using RealTimeChatApp.Domain.Entities;

namespace RealTimeChatApp.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enforce unique emails
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // GroupMember: composite key (UserId + GroupId)
            modelBuilder.Entity<GroupMember>()
                .HasKey(gm => new { gm.UserId, gm.GroupId });

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId);

            modelBuilder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId);
        }
    }
}
