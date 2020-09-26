using FileShare.Repository.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileShare.Repository.Mapping
{
    public class ExtensionPermittedMap : IEntityTypeConfiguration<ExtensionPermittedModel>
    {
        public void Configure(EntityTypeBuilder<ExtensionPermittedModel> builder)
        {
            builder.HasKey(c => c.Id);

            builder.ToTable("PermittedExtension");
            builder.Property(c => c.Id).HasColumnName("Id");
            builder.Property(c => c.Extension).HasColumnName("Extension").HasMaxLength(10);
            builder.Property(c => c.Description).HasColumnName("Description").HasMaxLength(400);
            builder.Property(c => c.MimeType).HasColumnName("MimeType").HasMaxLength(200).HasDefaultValue("application/octet-stream");
            builder.Property(c => c.CreationDateTime).HasColumnName("CreationDateTime").HasDefaultValueSql("getdate()");
        }
    }
}
