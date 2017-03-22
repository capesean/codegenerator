using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB.Models
{
    public class Lookup
    {
        [Key]
        [Required]
        public Guid LookupId { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Name { get; set; }

        public virtual ICollection<Field> LookupFields { get; set; } = new List<Field>();

        public virtual ICollection<LookupOption> LookupOptions { get; set; } = new List<LookupOption>();

        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        public Lookup()
        {
            LookupId = Guid.NewGuid();
        }
    }
}
