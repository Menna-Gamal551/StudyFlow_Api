using Microsoft.EntityFrameworkCore;
using StudyFlow_Api.Data.Models;

namespace StudyFlow_Api.Data
{
    public class AppdbContext(DbContextOptions<AppdbContext> options) : DbContext(options)
    {
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SubjectName).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.SubjectName).IsUnique();

            });
            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Subject).WithMany(e => e.Assignments).HasForeignKey(e => e.SubjectId).IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

                entity.Property(e => e.UserName).IsRequired().HasMaxLength(50);
            });


        }
    }
}
