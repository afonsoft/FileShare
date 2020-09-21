using System;
using System.ComponentModel.DataAnnotations;

namespace FireShare.Models
{
    public class FileModel
    {
        public Guid Id { get; set; }

        [Display(Name = "File Name")]
        public string UntrustedName { get; set; }

        [Display(Name = "File Hash")]
        public string Hash { get; set; }

        [Display(Name = "Size (bytes)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public long Size { get; set; }

        [Display(Name = "Uploaded (UTC)")]
        [DisplayFormat(DataFormatString = "{0:G}")]
        public DateTime UploadDT { get; set; }
    }
}
