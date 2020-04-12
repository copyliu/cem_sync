using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_systemjumps
    {
        [Key]
        public int id { get; set; }
        public int? fromRegionID { get; set; }
        public int? fromConstellationID { get; set; }
        public int fromSolarSystemID { get; set; }
        public int toSolarSystemID { get; set; }
        public int? toConstellationID { get; set; }
        public int? toRegionID { get; set; }

        [ForeignKey(nameof(fromConstellationID))]
        [InverseProperty(nameof(maps_constellations.maps_systemjumpsfromConstellation))]
        public virtual maps_constellations fromConstellation { get; set; }
        [ForeignKey(nameof(fromRegionID))]
        [InverseProperty(nameof(maps_regions.maps_systemjumpsfromRegion))]
        public virtual maps_regions fromRegion { get; set; }
        [ForeignKey(nameof(fromSolarSystemID))]
        [InverseProperty(nameof(maps_systems.maps_systemjumpsfromSolarSystem))]
        public virtual maps_systems fromSolarSystem { get; set; }
        [ForeignKey(nameof(toConstellationID))]
        [InverseProperty(nameof(maps_constellations.maps_systemjumpstoConstellation))]
        public virtual maps_constellations toConstellation { get; set; }
        [ForeignKey(nameof(toRegionID))]
        [InverseProperty(nameof(maps_regions.maps_systemjumpstoRegion))]
        public virtual maps_regions toRegion { get; set; }
        [ForeignKey(nameof(toSolarSystemID))]
        [InverseProperty(nameof(maps_systems.maps_systemjumpstoSolarSystem))]
        public virtual maps_systems toSolarSystem { get; set; }
    }
}
