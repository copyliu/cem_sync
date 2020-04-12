using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_struct
    {
        public maps_struct()
        {
            maps_struct_history = new HashSet<maps_struct_history>();
        }

        [Key]
        public long id { get; set; }
        public long alliance_id { get; set; }
        public int system_id { get; set; }
        public int invtype_id { get; set; }
        public double? structlevel { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? starttime { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? endtime { get; set; }
        public bool valid { get; set; }

        [ForeignKey(nameof(alliance_id))]
        [InverseProperty(nameof(maps_alliances.maps_struct))]
        public virtual maps_alliances alliance_ { get; set; }
        [ForeignKey(nameof(invtype_id))]
        [InverseProperty(nameof(evetypes.maps_struct))]
        public virtual evetypes invtype_ { get; set; }
        [ForeignKey(nameof(system_id))]
        [InverseProperty(nameof(maps_systems.maps_struct))]
        public virtual maps_systems system_ { get; set; }
        [InverseProperty("struct_")]
        public virtual ICollection<maps_struct_history> maps_struct_history { get; set; }
    }
}
