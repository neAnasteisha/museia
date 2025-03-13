using museia.Models;
using Microsoft.EntityFrameworkCore;

namespace museia.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet для кожної сутності
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<Complaint> Complaints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Post>().ToTable("Post");
            modelBuilder.Entity<Reaction>().ToTable("Reaction");
            modelBuilder.Entity<Complaint>().ToTable("Complaint");

            //це не чіпати, це треба, щоб воно працювало!!!!
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.User)
                .WithMany(u => u.Complaints)
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reactions)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.NoAction); 

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Complaints)
                .HasForeignKey(c => c.PostID)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.Post)
                .WithMany(p => p.Reactions)
                .HasForeignKey(r => r.PostID)
                .OnDelete(DeleteBehavior.Cascade); 
        }


    }
}
