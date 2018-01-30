using System;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models
{
    public class RelationshipDTO
    {
        [Required]
        public Guid RelationshipId { get; set; }

        [Required]
        public Guid ParentEntityId { get; set; }

        [Required]
        public Guid ChildEntityId { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string CollectionName { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string CollectionFriendlyName { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string ParentName { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string ParentFriendlyName { get; set; }

        [Required]
        public Guid ParentFieldId { get; set; }

        [Required]
        public bool DisplayListOnParent { get; set; }

        [Required]
        public bool Hierarchy { get; set; }

        [Required]
        public int SortOrder { get; set; }

        [Required]
        public RelationshipAncestorLimits RelationshipAncestorLimit { get; set; }

        [Required]
        public bool CascadeDelete { get; set; }

        public EntityDTO ParentEntity { get; set; }

        public FieldDTO ParentField { get; set; }

        public EntityDTO ChildEntity { get; set; }

    }

    public partial class ModelFactory
    {
        public RelationshipDTO Create(Relationship relationship)
        {
            if (relationship == null) return null;

            var relationshipDTO = new RelationshipDTO();

            relationshipDTO.RelationshipId = relationship.RelationshipId;
            relationshipDTO.ParentEntityId = relationship.ParentEntityId;
            relationshipDTO.ChildEntityId = relationship.ChildEntityId;
            relationshipDTO.CollectionName = relationship.CollectionName;
            relationshipDTO.CollectionFriendlyName = relationship.CollectionFriendlyName;
            relationshipDTO.ParentName = relationship.ParentName;
            relationshipDTO.ParentFriendlyName = relationship.ParentFriendlyName;
            relationshipDTO.ParentFieldId = relationship.ParentFieldId;
            relationshipDTO.DisplayListOnParent = relationship.DisplayListOnParent;
            relationshipDTO.Hierarchy = relationship.Hierarchy;
            relationshipDTO.SortOrder = relationship.SortOrder;
            relationshipDTO.RelationshipAncestorLimit = relationship.RelationshipAncestorLimit;
            relationshipDTO.CascadeDelete = relationship.CascadeDelete;
            relationshipDTO.ChildEntity = Create(relationship.ChildEntity);
            relationshipDTO.ParentEntity = Create(relationship.ParentEntity);
            relationshipDTO.ParentField = Create(relationship.ParentField);

            return relationshipDTO;
        }

        public void Hydrate(Relationship relationship, RelationshipDTO relationshipDTO)
        {
            relationship.ParentEntityId = relationshipDTO.ParentEntityId;
            relationship.ChildEntityId = relationshipDTO.ChildEntityId;
            relationship.CollectionName = relationshipDTO.CollectionName;
            relationship.CollectionFriendlyName = relationshipDTO.CollectionFriendlyName;
            relationship.ParentName = relationshipDTO.ParentName;
            relationship.ParentFriendlyName = relationshipDTO.ParentFriendlyName;
            relationship.ParentFieldId = relationshipDTO.ParentFieldId;
            relationship.DisplayListOnParent = relationshipDTO.DisplayListOnParent;
            relationship.Hierarchy = relationshipDTO.Hierarchy;
            relationship.SortOrder = relationshipDTO.SortOrder;
            relationship.RelationshipAncestorLimit = relationshipDTO.RelationshipAncestorLimit;
            relationship.CascadeDelete = relationshipDTO.CascadeDelete;
        }
    }
}
