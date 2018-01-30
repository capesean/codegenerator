using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB.Models
{
    public partial class Field
    {
        [Key]
        [Required]
        public Guid FieldId { get; set; }

        [Required]
        public Guid EntityId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(100)]
        public string Label { get; set; }

        [Required]
        public FieldType FieldType { get; set; }

        [Required]
        public int Length { get; set; }

        public byte? MinLength { get; set; }

        [Required]
        public byte Precision { get; set; }

        [Required]
        public byte Scale { get; set; }

        [Required]
        public bool KeyField { get; set; }

        [Required]
        public bool IsUnique { get; set; }

        [Required]
        public bool IsNullable { get; set; }

        [Required]
        public bool ShowInSearchResults { get; set; }

        [Required]
        public SearchType SearchType { get; set; }

        public int? SortPriority { get; set; }

        [Required]
        public bool SortDescending { get; set; }

        [Required]
        public int FieldOrder { get; set; }

        public Guid? LookupId { get; set; }

        [Required]
        public EditPageType EditPageType { get; set; }

        [MaxLength(50)]
        public string ControllerInsertOverride { get; set; }

        [MaxLength(50)]
        public string ControllerUpdateOverride { get; set; }

        [MaxLength(50)]
        public string EditPageDefault { get; set; }

        [MaxLength(500)]
        public string CalculatedFieldDefinition { get; set; }

        public virtual ICollection<RelationshipField> RelationshipFieldsAsChild { get; set; } = new List<RelationshipField>();

        public virtual ICollection<RelationshipField> RelationshipFieldsAsParent { get; set; } = new List<RelationshipField>();

        public virtual ICollection<Relationship> RelationshipsAsParentField { get; set; } = new List<Relationship>();

        [ForeignKey("EntityId")]
        public virtual Entity Entity { get; set; }

        [ForeignKey("LookupId")]
        public virtual Lookup Lookup { get; set; }

        public Field()
        {
            FieldId = Guid.NewGuid();
        }
    }
}
