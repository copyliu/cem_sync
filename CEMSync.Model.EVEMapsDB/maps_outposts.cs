using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_outposts
    {
        public maps_outposts()
        {
            maps_outposts_history = new HashSet<maps_outposts_history>();
        }

        [Key]
        public long stationID { get; set; }
        [Required]
        [StringLength(100)]
        public string stationName { get; set; }
        public int stationType_id { get; set; }
        public int solarSystem_id { get; set; }
        public long? corporation_id { get; set; }
        public bool visable { get; set; }
        public double? x { get; set; }
        public double? y { get; set; }
        public double? z { get; set; }

        [ForeignKey(nameof(corporation_id))]
        [InverseProperty(nameof(maps_corporation.maps_outposts))]
        public virtual maps_corporation corporation_ { get; set; }
        [ForeignKey(nameof(solarSystem_id))]
        [InverseProperty(nameof(maps_systems.maps_outposts))]
        public virtual maps_systems solarSystem_ { get; set; }
        [ForeignKey(nameof(stationType_id))]
        [InverseProperty(nameof(maps_stastationtypes.maps_outposts))]
        public virtual maps_stastationtypes stationType_ { get; set; }
        [InverseProperty("station_")]
        public virtual ICollection<maps_outposts_history> maps_outposts_history { get; set; }
    }
}
