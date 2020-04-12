using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_regions
    {
        public maps_regions()
        {
            maps_constellationjumpsfromRegion = new HashSet<maps_constellationjumps>();
            maps_constellationjumpstoRegion = new HashSet<maps_constellationjumps>();
            maps_constellations = new HashSet<maps_constellations>();
            maps_regionjumpsfromRegion = new HashSet<maps_regionjumps>();
            maps_regionjumpstoRegion = new HashSet<maps_regionjumps>();
            maps_systemjumpsfromRegion = new HashSet<maps_systemjumps>();
            maps_systemjumpstoRegion = new HashSet<maps_systemjumps>();
            maps_systems = new HashSet<maps_systems>();
        }

        [Key]
        public int regionID { get; set; }
        [Required]
        [StringLength(100)]
        public string regionName { get; set; }
        public double? x { get; set; }
        public double? y { get; set; }
        public double? z { get; set; }
        public double? xMin { get; set; }
        public double? xMax { get; set; }
        public double? yMin { get; set; }
        public double? yMax { get; set; }
        public double? zMin { get; set; }
        public double? zMax { get; set; }
        public int? factionID { get; set; }
        public double? radius { get; set; }

        [ForeignKey(nameof(factionID))]
        [InverseProperty(nameof(maps_factions.maps_regions))]
        public virtual maps_factions faction { get; set; }
        [InverseProperty(nameof(maps_constellationjumps.fromRegion))]
        public virtual ICollection<maps_constellationjumps> maps_constellationjumpsfromRegion { get; set; }
        [InverseProperty(nameof(maps_constellationjumps.toRegion))]
        public virtual ICollection<maps_constellationjumps> maps_constellationjumpstoRegion { get; set; }
        [InverseProperty("region")]
        public virtual ICollection<maps_constellations> maps_constellations { get; set; }
        [InverseProperty(nameof(maps_regionjumps.fromRegion))]
        public virtual ICollection<maps_regionjumps> maps_regionjumpsfromRegion { get; set; }
        [InverseProperty(nameof(maps_regionjumps.toRegion))]
        public virtual ICollection<maps_regionjumps> maps_regionjumpstoRegion { get; set; }
        [InverseProperty(nameof(maps_systemjumps.fromRegion))]
        public virtual ICollection<maps_systemjumps> maps_systemjumpsfromRegion { get; set; }
        [InverseProperty(nameof(maps_systemjumps.toRegion))]
        public virtual ICollection<maps_systemjumps> maps_systemjumpstoRegion { get; set; }
        [InverseProperty("region")]
        public virtual ICollection<maps_systems> maps_systems { get; set; }
    }
}
