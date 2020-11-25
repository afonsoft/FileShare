using FileShare.Repository.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FileShare.Repository.Mapping
{
    public class LoggerDownloadMap : IEntityTypeConfiguration<LoggerDownloadModel>
    {
        public void Configure(EntityTypeBuilder<LoggerDownloadModel> builder)
        {
            builder.HasKey(c => c.Id);

            builder.ToTable("LogFileDownload");
            builder.Property(c => c.Id).HasColumnName("Id");
            builder.Property(c => c.Name).HasColumnName("Name").HasMaxLength(400);
            builder.Property(c => c.StorageName).HasColumnName("StorageName").HasMaxLength(400);
            builder.Property(c => c.Type).HasColumnName("Type").HasMaxLength(200);
            builder.Property(c => c.Size).HasColumnName("Size");
            builder.Property(c => c.Ip).HasColumnName("IP").HasMaxLength(100);
            builder.Property(c => c.CreationDateTime).HasColumnName("CreationDateTime").HasDefaultValueSql("getdate()");

            builder.Property(c => c.CallingCode).HasColumnName("CallingCode").HasMaxLength(400);
            builder.Property(c => c.Postal).HasColumnName("Postal").HasMaxLength(400);
            builder.Property(c => c.Organisation).HasColumnName("Organisation").HasMaxLength(400);
            builder.Property(c => c.Longitude).HasColumnName("Longitude");
            builder.Property(c => c.Latitude).HasColumnName("Latitude");
            builder.Property(c => c.ContinentCode).HasColumnName("ContinentCode").HasMaxLength(400);
            builder.Property(c => c.ContinentName).HasColumnName("ContinentName").HasMaxLength(400);
            builder.Property(c => c.CountryCode).HasColumnName("CountryCode").HasMaxLength(400);
            builder.Property(c => c.CountryName).HasColumnName("CountryName").HasMaxLength(400);
            builder.Property(c => c.RegionCode).HasColumnName("RegionCode").HasMaxLength(400);
            builder.Property(c => c.Region).HasColumnName("Region").HasMaxLength(400);
            builder.Property(c => c.City).HasColumnName("City").HasMaxLength(400);
            builder.Property(c => c.Asn).HasColumnName("Asn").HasMaxLength(400);
            builder.Property(c => c.AsnName).HasColumnName("AsnName").HasMaxLength(400);
            builder.Property(c => c.AsnDomain).HasColumnName("AsnDomain").HasMaxLength(400);
            builder.Property(c => c.AsnRoute).HasColumnName("AsnRoute").HasMaxLength(400);
            builder.Property(c => c.AsnType).HasColumnName("AsnType").HasMaxLength(400);
            builder.Property(c => c.Languages).HasColumnName("Languages").HasMaxLength(400);
            builder.Property(c => c.TimeZone).HasColumnName("TimeZone").HasMaxLength(400);
        }
    }
}
