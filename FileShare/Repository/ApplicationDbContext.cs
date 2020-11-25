using Microsoft.EntityFrameworkCore;
using FileShare.Repository.Mapping;
using FileShare.Repository.Model;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace FileShare.Repository
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationIdentityUser, ApplicationIdentityRole, Guid>
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new FilesMap());
            builder.ApplyConfiguration(new ExtensionPermittedMap());
            builder.ApplyConfiguration(new FileUserMap());

            Seed(builder);
            base.OnModelCreating(builder);
        }

        private void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExtensionPermittedModel>()
            .HasData(
               new ExtensionPermittedModel
               {
                   Id = Guid.NewGuid(),
                   Extension = ".zip",
                   Description = "application/zip",
                   CreationDateTime = DateTime.UtcNow,
               }
            );
        }


        public DbSet<FileModel> Files { get; set; }
        public DbSet<ExtensionPermittedModel> PermittedExtension { get; set; }
        public DbSet<FileUserModel> FilesUsers { get; set; }
    }

    public class ApplicationIdentityUser : IdentityUser<Guid>
    {

    }

    public class ApplicationIdentityRole : IdentityRole<Guid>
    {

    }
}
