using FileShare.Repository.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

            builder.HasOne(c => c.File).WithMany().HasForeignKey("FileId").OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(c => c.User).WithMany().HasForeignKey("UserId").OnDelete(DeleteBehavior.Cascade);
        }
    }
}