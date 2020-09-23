using System;
using System.ComponentModel.DataAnnotations;

namespace FireShare.Models
{
    public class FileModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Name")]
        public string UntrustedName { get; set; }

        [Display(Name = "File Name")]
        public string TrustedName { get; set; }

        [Display(Name = "File Hash")]
        public string Hash { get; set; }

        [Display(Name = "File Path")]
        public string Path { get; set; }

        [Display(Name = "File Type")]
        public string Type { get; set; }

        [Display(Name = "Size")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public long Size { get; set; }

        [Display(Name = "Create (UTC)")]
        [DisplayFormat(DataFormatString = "{0:G}")]
        public DateTime UploadDT { get; set; }
    }
}
