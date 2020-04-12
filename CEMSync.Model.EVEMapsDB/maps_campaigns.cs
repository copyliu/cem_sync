using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_campaigns
    {
        public maps_campaigns()
        {
            maps_campaignatk = new HashSet<maps_campaignatk>();
            maps_campaigndef = new HashSet<maps_campaigndef>();
            maps_campaigns_history = new HashSet<maps_campaigns_history>();
        }

        [Key]
        public long id { get; set; }
        public int eventtype { get; set; }
        public int system_id { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime starttime { get; set; }
        public bool valid { get; set; }
        public long? struct_id { get; set; }

        [ForeignKey(nameof(system_id))]
        [InverseProperty(nameof(maps_systems.maps_campaigns))]
        public virtual maps_systems system_ { get; set; }
        [InverseProperty("campaigns_")]
        public virtual ICollection<maps_campaignatk> maps_campaignatk { get; set; }
        [InverseProperty("campaigns_")]
        public virtual ICollection<maps_campaigndef> maps_campaigndef { get; set; }
        [InverseProperty("campaign_")]
        public virtual ICollection<maps_campaigns_history> maps_campaigns_history { get; set; }
    }
}
