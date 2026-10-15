using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Context
{
    public class FundooContext : DbContext
    {
        public FundooContext(
            DbContextOptions<FundooContext> options) : base(options)
        {
        }
        public DbSet<UserEntity> Users { get; set; }

        // NEW
        public DbSet<NoteEntity> Notes { get; set; }

        // NEW
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NoteEntity>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}