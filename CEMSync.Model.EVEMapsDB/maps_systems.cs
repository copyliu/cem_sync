using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_systems
    {
        public maps_systems()
        {
            maps_campaigns = new HashSet<maps_campaigns>();
            maps_factions = new HashSet<maps_factions>();
            maps_jumpstatic = new HashSet<maps_jumpstatic>();
            maps_killstatic = new HashSet<maps_killstatic>();
            maps_outposts = new HashSet<maps_outposts>();
            maps_sovereignty_history = new HashSet<maps_sovereignty_history>();
            maps_struct = new HashSet<maps_struct>();
            maps_systemjumpsfromSolarSystem = new HashSet<maps_systemjumps>();
            maps_systemjumpstoSolarSystem = new HashSet<maps_systemjumps>();
        }

        [Key]
        public int solarSystemID { get; set; }
        public int? regionID { get; set; }
        public int? constellationID { get; set; }
        [Required]
        [StringLength(100)]
        public string solarSystemName { get; set; }
        public double? x { get; set; }
        public double? y { get; set; }
        public double? z { get; set; }
        public double? xMin { get; set; }
        public double? xMax { get; set; }
        public double? yMin { get; set; }
        public double? yMax { get; set; }
        public double? zMin { get; set; }
        public double? zMax { get; set; }
        public double? luminosity { get; set; }
        public short? border { get; set; }
        public short? fringe { get; set; }
        public short? corridor { get; set; }
        public short? hub { get; set; }
        public short? international { get; set; }
        public short? regional { get; set; }
        public short? constellation { get; set; }
        public double? security { get; set; }
        public int? factionID { get; set; }
        public double? radius { get; set; }
        public int? sunTypeID { get; set; }
        [StringLength(2)]
        public string securityClass { get; set; }

        [ForeignKey(nameof(constellationID))]
        [InverseProperty(nameof(maps_constellations.maps_systems))]
        public virtual maps_constellations constellationNavigation { get; set; }
        [ForeignKey(nameof(factionID))]
        [InverseProperty("maps_systems")]
        public virtual maps_factions faction { get; set; }
        [ForeignKey(nameof(regionID))]
        [InverseProperty(nameof(maps_regions.maps_systems))]
        public virtual maps_regions region { get; set; }
        [InverseProperty("solarSystem_")]
        public virtual maps_sovereignty maps_sovereignty { get; set; }
        [InverseProperty("system_")]
        public virtual maps_systemgrid maps_systemgrid { get; set; }
        [InverseProperty("system_")]
        public virtual ICollection<maps_campaigns> maps_campaigns { get; set; }
        [InverseProperty("solarSystem")]
        public virtual ICollection<maps_factions> maps_factions { get; set; }
        [InverseProperty("system_")]
        public virtual ICollection<maps_jumpstatic> maps_jumpstatic { get; set; }
        [InverseProperty("system_")]
        public virtual ICollection<maps_killstatic> maps_killstatic { get; set; }
        [InverseProperty("solarSystem_")]
        public virtual ICollection<maps_outposts> maps_outposts { get; set; }
        [InverseProperty("system_")]
        public virtual ICollection<maps_sovereignty_history> maps_sovereignty_history { get; set; }
        [InverseProperty("system_")]
        public virtual ICollection<maps_struct> maps_struct { get; set; }
        [InverseProperty(nameof(maps_systemjumps.fromSolarSystem))]
        public virtual ICollection<maps_systemjumps> maps_systemjumpsfromSolarSystem { get; set; }
        [InverseProperty(nameof(maps_systemjumps.toSolarSystem))]
        public virtual ICollection<maps_systemjumps> maps_systemjumpstoSolarSystem { get; set; }
    }
}
