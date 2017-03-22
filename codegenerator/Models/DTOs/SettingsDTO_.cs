using System;
using System.Collections.Generic;

namespace WEB.Models
{
    public partial class SettingsDTO
    {
        public List<EnumDTO> AuthorizationType { get; set; }
        public List<EnumDTO> CodeType { get; set; }
        public List<EnumDTO> EditPageType { get; set; }
        public List<EnumDTO> EntityType { get; set; }
        public List<EnumDTO> FieldType { get; set; }
        public List<EnumDTO> RelationshipAncestorLimits { get; set; }
        public List<EnumDTO> SearchType { get; set; }

        public SettingsDTO()
        {
            AuthorizationType = new List<EnumDTO>();
            foreach (AuthorizationType type in Enum.GetValues(typeof(AuthorizationType)))
                AuthorizationType.Add(new EnumDTO { Id = (int)type, Name = type.ToString(), Label = type.Label() });

            CodeType = new List<EnumDTO>();
            foreach (CodeType type in Enum.GetValues(typeof(CodeType)))
                CodeType.Add(new EnumDTO { Id = (int)type, Name = type.ToString(), Label = type.Label() });

            EditPageType = new List<EnumDTO>();
            foreach (EditPageType type in Enum.GetValues(typeof(EditPageType)))
                EditPageType.Add(new EnumDTO { Id = (int)type, Name = type.ToString(), Label = type.Label() });

            EntityType = new List<EnumDTO>();
            foreach (EntityType type in Enum.GetValues(typeof(EntityType)))
                EntityType.Add(new EnumDTO { Id = (int)type, Name = type.ToString(), Label = type.Label() });

            FieldType = new List<EnumDTO>();
            foreach (FieldType type in Enum.GetValues(typeof(FieldType)))
                FieldType.Add(new EnumDTO { Id = (int)type, Name = type.ToString(), Label = type.Label() });

            RelationshipAncestorLimits = new List<EnumDTO>();
            foreach (RelationshipAncestorLimits type in Enum.GetValues(typeof(RelationshipAncestorLimits)))
                RelationshipAncestorLimits.Add(new EnumDTO { Id = (int)type, Name = type.ToString(), Label = type.Label() });

            SearchType = new List<EnumDTO>();
            foreach (SearchType type in Enum.GetValues(typeof(SearchType)))
                SearchType.Add(new EnumDTO { Id = (int)type, Name = type.ToString(), Label = type.Label() });

        }
    }
}
