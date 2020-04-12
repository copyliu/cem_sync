using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_regionjumps
    {
        [Key]
        public int id { get; set; }
        public int fromRegionID { get; set; }
        public int toRegionID { get; set; }

        [ForeignKey(nameof(fromRegionID))]
        [InverseProperty(nameof(maps_regions.maps_regionjumpsfromRegion))]
        public virtual maps_regions fromRegion { get; set; }
        [ForeignKey(nameof(toRegionID))]
        [InverseProperty(nameof(maps_regions.maps_regionjumpstoRegion))]
        public virtual maps_regions toRegion { get; set; }
    }
}
