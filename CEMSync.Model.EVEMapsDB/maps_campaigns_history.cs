using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_campaigns_history
    {
        public maps_campaigns_history()
        {
            maps_eventhistory = new HashSet<maps_eventhistory>();
        }

        [Key]
        public int id { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime date { get; set; }
        public int flag { get; set; }
        public long campaign_id { get; set; }

        [ForeignKey(nameof(campaign_id))]
        [InverseProperty(nameof(maps_campaigns.maps_campaigns_history))]
        public virtual maps_campaigns campaign_ { get; set; }
        [InverseProperty("cam_his_")]
        public virtual ICollection<maps_eventhistory> maps_eventhistory { get; set; }
    }
}
