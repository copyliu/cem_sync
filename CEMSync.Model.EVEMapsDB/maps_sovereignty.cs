using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_sovereignty
    {
        [Key]
        public int solarSystem_id { get; set; }
        public long? alliance_id { get; set; }
        public int? faction_id { get; set; }
        public long? corporation_id { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime getdate { get; set; }

        [ForeignKey(nameof(alliance_id))]
        [InverseProperty(nameof(maps_alliances.maps_sovereignty))]
        public virtual maps_alliances alliance_ { get; set; }
        [ForeignKey(nameof(corporation_id))]
        [InverseProperty(nameof(maps_corporation.maps_sovereignty))]
        public virtual maps_corporation corporation_ { get; set; }
        [ForeignKey(nameof(faction_id))]
        [InverseProperty(nameof(maps_factions.maps_sovereignty))]
        public virtual maps_factions faction_ { get; set; }
        [ForeignKey(nameof(solarSystem_id))]
        [InverseProperty(nameof(maps_systems.maps_sovereignty))]
        public virtual maps_systems solarSystem_ { get; set; }
    }
}
