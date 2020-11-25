using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileShare.Repository.Model
{

    public class LoggerDownloadModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string StorageName { get; set; }
        public string Type { get; set; }
        public long Size { get; set; }
        public DateTime CreationDateTime { get; set; }
        public Guid? UserId { get; set; }

        public string CallingCode { get; set; }

        public string Postal { get; set; }

        public string Organisation { get; set; }

        public double? Longitude { get; set; }

        public double? Latitude { get; set; }

        public string ContinentCode { get; set; }

        public string ContinentName { get; set; }

        public string CountryCode { get; set; }

        public string CountryName { get; set; }

        public string RegionCode { get; set; }

        public string Region { get; set; }

        public string City { get; set; }

        public string Ip { get; set; }
        public string Asn { get; set; }

        public string AsnName { get; set; }

        public string AsnDomain { get; set; }

        public string AsnRoute { get; set; }

        public string AsnType { get; set; }
        public string Languages { get; set; }
        public string TimeZone { get; set; }
        public string Error { get; set; }

    }
}
