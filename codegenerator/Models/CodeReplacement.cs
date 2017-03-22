using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB.Models
{
    public class CodeReplacement
    {
        [Key]
        [Required]
        public Guid CodeReplacementId { get; set; }

        [Required]
        public Guid EntityId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Purpose { get; set; }

        [Required]
        public CodeType CodeType { get; set; }

        [Required]
        public bool Disabled { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string FindCode { get; set; }

        public string ReplacementCode { get; set; }

        [Required]
        public int SortOrder { get; set; }

        [ForeignKey("EntityId")]
        public virtual Entity Entity { get; set; }

        public CodeReplacement()
        {
            CodeReplacementId = Guid.NewGuid();
        }
    }
}
