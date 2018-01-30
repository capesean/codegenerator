using System;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models
{
    public class RelationshipFieldDTO
    {
        [Required]
        public Guid RelationshipFieldId { get; set; }

        [Required]
        public Guid RelationshipId { get; set; }

        [Required]
        public Guid ParentFieldId { get; set; }

        [Required]
        public Guid ChildFieldId { get; set; }

        public FieldDTO ChildField { get; set; }

        public FieldDTO ParentField { get; set; }

        public RelationshipDTO Relationship { get; set; }

    }

    public partial class ModelFactory
    {
        public RelationshipFieldDTO Create(RelationshipField relationshipField)
        {
            if (relationshipField == null) return null;

            var relationshipFieldDTO = new RelationshipFieldDTO();

            relationshipFieldDTO.RelationshipFieldId = relationshipField.RelationshipFieldId;
            relationshipFieldDTO.RelationshipId = relationshipField.RelationshipId;
            relationshipFieldDTO.ParentFieldId = relationshipField.ParentFieldId;
            relationshipFieldDTO.ChildFieldId = relationshipField.ChildFieldId;
            relationshipFieldDTO.ParentField = Create(relationshipField.ParentField);
            relationshipFieldDTO.ChildField = Create(relationshipField.ChildField);
            relationshipFieldDTO.Relationship = Create(relationshipField.Relationship);

            return relationshipFieldDTO;
        }

        public void Hydrate(RelationshipField relationshipField, RelationshipFieldDTO relationshipFieldDTO)
        {
            relationshipField.RelationshipId = relationshipFieldDTO.RelationshipId;
            relationshipField.ParentFieldId = relationshipFieldDTO.ParentFieldId;
            relationshipField.ChildFieldId = relationshipFieldDTO.ChildFieldId;
        }
    }
}
