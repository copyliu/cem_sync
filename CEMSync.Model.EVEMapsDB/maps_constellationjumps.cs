using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_constellationjumps
    {
        [Key]
        public int id { get; set; }
        public int? fromRegionID { get; set; }
        public int fromConstellationID { get; set; }
        public int toConstellationID { get; set; }
        public int? toRegionID { get; set; }

        [ForeignKey(nameof(fromConstellationID))]
        [InverseProperty(nameof(maps_constellations.maps_constellationjumpsfromConstellation))]
        public virtual maps_constellations fromConstellation { get; set; }
        [ForeignKey(nameof(fromRegionID))]
        [InverseProperty(nameof(maps_regions.maps_constellationjumpsfromRegion))]
        public virtual maps_regions fromRegion { get; set; }
        [ForeignKey(nameof(toConstellationID))]
        [InverseProperty(nameof(maps_constellations.maps_constellationjumpstoConstellation))]
        public virtual maps_constellations toConstellation { get; set; }
        [ForeignKey(nameof(toRegionID))]
        [InverseProperty(nameof(maps_regions.maps_constellationjumpstoRegion))]
        public virtual maps_regions toRegion { get; set; }
    }
}
