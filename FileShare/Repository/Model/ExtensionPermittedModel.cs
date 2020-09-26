using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileShare.Repository.Model
{
    public class ExtensionPermittedModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        public string Extension { get; set; }
        public string Description { get; set; }
        public string MimeType { get; set; }
        public DateTime CreationDateTime { get; set; }
    }
}
