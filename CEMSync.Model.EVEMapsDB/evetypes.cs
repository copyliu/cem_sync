using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class evetypes
    {
        public evetypes()
        {
            maps_struct = new HashSet<maps_struct>();
        }

        [Key]
        public int typeID { get; set; }
        public int? groupID { get; set; }
        [Required]
        [StringLength(300)]
        public string typeName { get; set; }
        public string description { get; set; }
        public double? mass { get; set; }
        public double? volume { get; set; }
        public double? capacity { get; set; }
        public int? portionSize { get; set; }
        public int? raceID { get; set; }
        [Column(TypeName = "numeric(21,4)")]
        public decimal? basePrice { get; set; }
        public bool? published { get; set; }
        public int? marketGroupID { get; set; }
        public int? iconID { get; set; }
        [Required]
        [StringLength(300)]
        public string typeName_en { get; set; }
        [StringLength(9000)]
        public string description_en { get; set; }
        [Required]
        [StringLength(300)]
        public string typeName_de { get; set; }
        public string description_de { get; set; }
        [Required]
        [StringLength(300)]
        public string typeName_es { get; set; }
        public string description_es { get; set; }
        [Required]
        [StringLength(300)]
        public string typeName_fr { get; set; }
        public string description_fr { get; set; }
        [Required]
        [StringLength(300)]
        public string typeName_it { get; set; }
        public string description_it { get; set; }
        [Required]
        [StringLength(300)]
        public string typeName_ja { get; set; }
        public string description_ja { get; set; }
        [Required]
        [StringLength(300)]
        public string typeName_ru { get; set; }
        public string description_ru { get; set; }

        [ForeignKey(nameof(marketGroupID))]
        [InverseProperty(nameof(marketgroup.evetypes))]
        public virtual marketgroup marketGroup { get; set; }
        [InverseProperty("invtype_")]
        public virtual ICollection<maps_struct> maps_struct { get; set; }
    }
}
