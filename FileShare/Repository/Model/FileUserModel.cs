using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileShare.Repository.Model
{
    public class FileUserModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        public Guid FileId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreationDateTime { get; set; }
    }
}
