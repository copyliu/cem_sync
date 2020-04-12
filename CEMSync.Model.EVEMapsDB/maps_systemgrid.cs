using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_systemgrid
    {
        [Key]
        public int system_id { get; set; }
        [Required]
        [StringLength(10)]
        public string grid { get; set; }

        [ForeignKey(nameof(system_id))]
        [InverseProperty(nameof(maps_systems.maps_systemgrid))]
        public virtual maps_systems system_ { get; set; }
    }
}
