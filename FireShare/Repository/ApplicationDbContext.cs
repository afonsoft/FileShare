using Microsoft.EntityFrameworkCore;
using FileShare.Repository.Mapping;
using FileShare.Repository.Model;

namespace FileShare.Repository
{
    public class ApplicationDbContext : DbContext
    { 
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            try
            {
                Database.Migrate();
            }
            catch
            {
                //Ignore
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new FilesMap());
            
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<FileModel> Files { get; set; }
    }
}
