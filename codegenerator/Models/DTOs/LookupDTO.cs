using System;
using System.ComponentModel.DataAnnotations;

namespace WEB.Models
{
    public class LookupDTO
    {
        [Required]
        public Guid LookupId { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [MaxLength(50)]
        public string Name { get; set; }

        public ProjectDTO Project { get; set; }

    }

    public partial class ModelFactory
    {
        public LookupDTO Create(Lookup lookup)
        {
            if (lookup == null) return null;

            var lookupDTO = new LookupDTO();

            lookupDTO.LookupId = lookup.LookupId;
            lookupDTO.ProjectId = lookup.ProjectId;
            lookupDTO.Name = lookup.Name;
            lookupDTO.Project = Create(lookup.Project);

            return lookupDTO;
        }

        public void Hydrate(Lookup lookup, LookupDTO lookupDTO)
        {
            lookup.ProjectId = lookupDTO.ProjectId;
            lookup.Name = lookupDTO.Name;
        }
    }
}
