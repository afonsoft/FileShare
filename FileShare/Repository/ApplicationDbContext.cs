﻿using Microsoft.EntityFrameworkCore;
using FileShare.Repository.Mapping;
using FileShare.Repository.Model;
using System;

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
            modelBuilder.ApplyConfiguration(new ExtensionPermittedMap());

            Seed(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        private void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExtensionPermittedModel>()
       .HasData(
               new ExtensionPermittedModel
               {
                   Id = Guid.NewGuid(),
                   Extension = ".zip",
                   CreationDateTime = DateTime.Now,
                   MimeType = "application/zip"
               }
            );
        }


        public DbSet<FileModel> Files { get; set; }
        public DbSet<ExtensionPermittedModel> PermittedExtension { get; set; }
    }
}
