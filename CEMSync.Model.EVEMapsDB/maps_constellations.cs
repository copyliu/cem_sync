using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_constellations
    {
        public maps_constellations()
        {
            maps_constellationjumpsfromConstellation = new HashSet<maps_constellationjumps>();
            maps_constellationjumpstoConstellation = new HashSet<maps_constellationjumps>();
            maps_systemjumpsfromConstellation = new HashSet<maps_systemjumps>();
            maps_systemjumpstoConstellation = new HashSet<maps_systemjumps>();
            maps_systems = new HashSet<maps_systems>();
        }

        public int? regionID { get; set; }
        [Key]
        public int constellationID { get; set; }
        [Required]
        [StringLength(100)]
        public string constellationName { get; set; }
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
        [InverseProperty(nameof(maps_factions.maps_constellations))]
        public virtual maps_factions faction { get; set; }
        [ForeignKey(nameof(regionID))]
        [InverseProperty(nameof(maps_regions.maps_constellations))]
        public virtual maps_regions region { get; set; }
        [InverseProperty(nameof(maps_constellationjumps.fromConstellation))]
        public virtual ICollection<maps_constellationjumps> maps_constellationjumpsfromConstellation { get; set; }
        [InverseProperty(nameof(maps_constellationjumps.toConstellation))]
        public virtual ICollection<maps_constellationjumps> maps_constellationjumpstoConstellation { get; set; }
        [InverseProperty(nameof(maps_systemjumps.fromConstellation))]
        public virtual ICollection<maps_systemjumps> maps_systemjumpsfromConstellation { get; set; }
        [InverseProperty(nameof(maps_systemjumps.toConstellation))]
        public virtual ICollection<maps_systemjumps> maps_systemjumpstoConstellation { get; set; }
        [InverseProperty("constellationNavigation")]
        public virtual ICollection<maps_systems> maps_systems { get; set; }
    }
}
