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
        public DateTime CreationDateTime { get; set; }

        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationIdentityUser User { get; set; }

        public Guid FileId { get; set; }
        [ForeignKey("FileId")]
        public virtual FileModel File {get;set;}
    }
}
