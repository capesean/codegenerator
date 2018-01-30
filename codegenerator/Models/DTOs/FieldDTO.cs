using System;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models
{
    public class FieldDTO
    {
        [Required]
        public Guid FieldId { get; set; }

        [Required]
        public Guid EntityId { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string Name { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
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

        public LookupDTO Lookup { get; set; }

        public EntityDTO Entity { get; set; }

    }

    public partial class ModelFactory
    {
        public FieldDTO Create(Field field)
        {
            if (field == null) return null;

            var fieldDTO = new FieldDTO();

            fieldDTO.FieldId = field.FieldId;
            fieldDTO.EntityId = field.EntityId;
            fieldDTO.Name = field.Name;
            fieldDTO.Label = field.Label;
            fieldDTO.FieldType = field.FieldType;
            fieldDTO.Length = field.Length;
            fieldDTO.MinLength = field.MinLength;
            fieldDTO.Precision = field.Precision;
            fieldDTO.Scale = field.Scale;
            fieldDTO.KeyField = field.KeyField;
            fieldDTO.IsUnique = field.IsUnique;
            fieldDTO.IsNullable = field.IsNullable;
            fieldDTO.ShowInSearchResults = field.ShowInSearchResults;
            fieldDTO.SearchType = field.SearchType;
            fieldDTO.SortPriority = field.SortPriority;
            fieldDTO.SortDescending = field.SortDescending;
            fieldDTO.FieldOrder = field.FieldOrder;
            fieldDTO.LookupId = field.LookupId;
            fieldDTO.EditPageType = field.EditPageType;
            fieldDTO.ControllerInsertOverride = field.ControllerInsertOverride;
            fieldDTO.ControllerUpdateOverride = field.ControllerUpdateOverride;
            fieldDTO.EditPageDefault = field.EditPageDefault;
            fieldDTO.CalculatedFieldDefinition = field.CalculatedFieldDefinition;
            fieldDTO.Lookup = Create(field.Lookup);
            fieldDTO.Entity = Create(field.Entity);

            return fieldDTO;
        }

        public void Hydrate(Field field, FieldDTO fieldDTO)
        {
            field.EntityId = fieldDTO.EntityId;
            field.Name = fieldDTO.Name;
            field.Label = fieldDTO.Label;
            field.FieldType = fieldDTO.FieldType;
            field.Length = fieldDTO.Length;
            field.MinLength = fieldDTO.MinLength;
            field.Precision = fieldDTO.Precision;
            field.Scale = fieldDTO.Scale;
            field.KeyField = fieldDTO.KeyField;
            field.IsUnique = fieldDTO.IsUnique;
            field.IsNullable = fieldDTO.IsNullable;
            field.ShowInSearchResults = fieldDTO.ShowInSearchResults;
            field.SearchType = fieldDTO.SearchType;
            field.SortPriority = fieldDTO.SortPriority;
            field.SortDescending = fieldDTO.SortDescending;
            field.FieldOrder = fieldDTO.FieldOrder;
            field.LookupId = fieldDTO.LookupId;
            field.EditPageType = fieldDTO.EditPageType;
            field.ControllerInsertOverride = fieldDTO.ControllerInsertOverride;
            field.ControllerUpdateOverride = fieldDTO.ControllerUpdateOverride;
            field.EditPageDefault = fieldDTO.EditPageDefault;
            field.CalculatedFieldDefinition = fieldDTO.CalculatedFieldDefinition;
        }
    }
}
