using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FireShare.Repository.Model
{
    public class FileModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public long Size { get; set; }
        public string Email { get; set; }
        public string IP { get; set; }
        public string Hash { get; set; }
        public DateTime CreationDateTime { get; set; }
    }
}
