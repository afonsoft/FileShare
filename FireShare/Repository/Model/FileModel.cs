using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileShare.Repository.Model
{
    public class FileModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string StorageName { get; set; }
        public string Type { get; set; }
        public long Size { get; set; }
        public string IP { get; set; }
        public string Hash { get; set; }
        public DateTime CreationDateTime { get; set; }

        public override string ToString()
        {
            return Hash;
        }
    }
}
