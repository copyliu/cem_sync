using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_jumpstatic
    {
        [Key]
        public int id { get; set; }
        public int system_id { get; set; }
        public int jumps { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime dataTime { get; set; }

        [ForeignKey(nameof(system_id))]
        [InverseProperty(nameof(maps_systems.maps_jumpstatic))]
        public virtual maps_systems system_ { get; set; }
    }
}
