using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class marketgroup
    {
        public marketgroup()
        {
            InverseparentGroup = new HashSet<marketgroup>();
            evetypes = new HashSet<evetypes>();
        }

        [Key]
        public int marketGroupID { get; set; }
        public int? parentGroupID { get; set; }
        [Required]
        public string marketGroupName { get; set; }
        public string description { get; set; }
        public int? iconID { get; set; }
        public bool? hasTypes { get; set; }
        [Required]
        [StringLength(300)]
        public string marketGroupName_en { get; set; }
        [StringLength(9000)]
        public string description_en { get; set; }

        [ForeignKey(nameof(parentGroupID))]
        [InverseProperty(nameof(marketgroup.InverseparentGroup))]
        public virtual marketgroup parentGroup { get; set; }
        [InverseProperty(nameof(marketgroup.parentGroup))]
        public virtual ICollection<marketgroup> InverseparentGroup { get; set; }
        [InverseProperty("marketGroup")]
        public virtual ICollection<evetypes> evetypes { get; set; }
    }
}
