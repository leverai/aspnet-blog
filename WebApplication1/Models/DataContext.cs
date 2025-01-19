using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.Domain;
using WebApplication1.Models.ViewModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace WebApplication1.Models
{
    
    public class DataContext : DbContext
    {
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<User> Users { get; set; }
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
            Database.EnsureCreated();  
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.User) 
                .WithMany(u => u.Posts) 
                .HasForeignKey(p => p.AuthorID) 
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
