using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FireShare.Repository.Model;

namespace FireShare.Repository.Mapping
{
    public class FilesMap : IEntityTypeConfiguration<FileModel>
    {
        public void Configure(EntityTypeBuilder<FileModel> builder)
        {
            builder.HasKey(c => c.Id);

            builder.ToTable("Files");
            builder.Property(c => c.Id).HasColumnName("Id");
            builder.Property(c => c.Email).HasColumnName("Email").HasMaxLength(255);
            builder.Property(c => c.Name).HasColumnName("Name").HasMaxLength(300);
            builder.Property(c => c.Type).HasColumnName("Type").HasMaxLength(250);
            builder.Property(c => c.Size).HasColumnName("Size");
            builder.Property(c => c.IP).HasColumnName("IP").HasMaxLength(100);
            builder.Property(c => c.Hash).HasColumnName("Hash").HasMaxLength(4000);
            builder.Property(c => c.CreationDateTime).HasColumnName("CreationDateTime").HasDefaultValueSql("getdate()");
        }
    }
}
