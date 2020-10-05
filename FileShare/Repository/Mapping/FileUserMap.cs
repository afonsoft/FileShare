using FileShare.Repository.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileShare.Repository.Mapping
{
    public class FileUserMap : IEntityTypeConfiguration<FileUserModel>
    {
        public void Configure(EntityTypeBuilder<FileUserModel> builder)
        {
            builder.HasKey(c => c.Id);

            builder.ToTable("FilesUsers");
            builder.Property(c => c.Id).HasColumnName("Id");
            builder.Property(c => c.FileId).HasColumnName("FileId");
            builder.Property(c => c.UserId).HasColumnName("UserId");
            builder.Property(c => c.CreationDateTime).HasColumnName("CreationDateTime").HasDefaultValueSql("getdate()");
        }
    }
}
