using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB.Models
{
    public class LookupOption
    {
        [Key]
        [Required]
        public Guid LookupOptionId { get; set; }

        [Required]
        public Guid LookupId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string FriendlyName { get; set; }

        public int? Value { get; set; }

        [Required]
        public byte SortOrder { get; set; }

        [ForeignKey("LookupId")]
        public virtual Lookup Lookup { get; set; }

        public LookupOption()
        {
            LookupOptionId = Guid.NewGuid();
        }
    }
}
