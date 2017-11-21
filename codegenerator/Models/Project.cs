using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB.Models
{
    public class Project
    {
        [Key]
        [Required]
        public Guid ProjectId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        [Index("IX_Project_Name", IsUnique = true, Order = 0)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(250)]
        public string RootPath { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(20)]
        public string Namespace { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(20)]
        public string AngularModuleName { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(20)]
        public string AngularDirectivePrefix { get; set; }

        [Required]
        public bool Bootstrap3 { get; set; }

        [Required]
        public bool ExcludeTypes { get; set; }

        [MaxLength(50)]
        public string UrlPrefix { get; set; }

        [Required]
        public bool UseStringAuthorizeAttributes { get; set; }

        public virtual ICollection<Entity> Entities { get; set; } = new List<Entity>();

        public virtual ICollection<Lookup> Lookups { get; set; } = new List<Lookup>();

        public Project()
        {
            ProjectId = Guid.NewGuid();
        }
    }
}
