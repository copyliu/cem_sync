using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_factions
    {
        public maps_factions()
        {
            maps_constellations = new HashSet<maps_constellations>();
            maps_regions = new HashSet<maps_regions>();
            maps_sovereignty = new HashSet<maps_sovereignty>();
            maps_sovereignty_historyfromfaction_ = new HashSet<maps_sovereignty_history>();
            maps_sovereignty_historytofaction_ = new HashSet<maps_sovereignty_history>();
            maps_systems = new HashSet<maps_systems>();
        }

        [Key]
        public int factionID { get; set; }
        [Required]
        [StringLength(100)]
        public string factionName { get; set; }
        [Required]
        [StringLength(1000)]
        public string description { get; set; }
        public int? raceIDs { get; set; }
        public int? solarSystemID { get; set; }
        public int? corporationID { get; set; }
        public double? sizeFactor { get; set; }
        public short? stationCount { get; set; }
        public short? stationSystemCount { get; set; }
        public int? militiaCorporationID { get; set; }
        public short? iconID { get; set; }

        [ForeignKey(nameof(solarSystemID))]
        [InverseProperty("maps_factions")]
        public virtual maps_systems solarSystem { get; set; }
        [InverseProperty("faction")]
        public virtual ICollection<maps_constellations> maps_constellations { get; set; }
        [InverseProperty("faction")]
        public virtual ICollection<maps_regions> maps_regions { get; set; }
        [InverseProperty("faction_")]
        public virtual ICollection<maps_sovereignty> maps_sovereignty { get; set; }
        [InverseProperty(nameof(maps_sovereignty_history.fromfaction_))]
        public virtual ICollection<maps_sovereignty_history> maps_sovereignty_historyfromfaction_ { get; set; }
        [InverseProperty(nameof(maps_sovereignty_history.tofaction_))]
        public virtual ICollection<maps_sovereignty_history> maps_sovereignty_historytofaction_ { get; set; }
        [InverseProperty("faction")]
        public virtual ICollection<maps_systems> maps_systems { get; set; }
    }
}
