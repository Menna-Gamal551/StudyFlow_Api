using Microsoft.EntityFrameworkCore;

namespace StudyFlow_Api.Models
{
    public class AppdbContext(DbContextOptions<AppdbContext> options):DbContext(options)
    {
        public DbSet<Subject>Subjects { get; set; }
        public DbSet<Assignment>Assignments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(e=>e.Id);
                entity.Property(e => e.SubjectName).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.SubjectName).IsUnique();
              
            });
            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Subject).WithMany(e => e.Assignments).HasForeignKey(e=>e.SubjectId).IsRequired();
            });



        }
    }
}
