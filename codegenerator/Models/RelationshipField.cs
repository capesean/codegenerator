using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB.Models
{
    public class RelationshipField
    {
        [Key]
        [Required]
        public Guid RelationshipFieldId { get; set; }

        [Required]
        public Guid RelationshipId { get; set; }

        [Required]
        public Guid ParentFieldId { get; set; }

        [Required]
        public Guid ChildFieldId { get; set; }

        [ForeignKey("ChildFieldId")]
        public virtual Field ChildField { get; set; }

        [ForeignKey("ParentFieldId")]
        public virtual Field ParentField { get; set; }

        [ForeignKey("RelationshipId")]
        public virtual Relationship Relationship { get; set; }

        public RelationshipField()
        {
            RelationshipFieldId = Guid.NewGuid();
        }
    }
}
