using Microsoft.EntityFrameworkCore;
using RealTimeChatApp.Domain.Entities;

namespace RealTimeChatApp.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }  // ✅ Register this table

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Unique constraint on email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // ✅ Primary key for GroupMember
            modelBuilder.Entity<GroupMember>()
                .HasKey(gm => gm.Id);
        }
    }
}
