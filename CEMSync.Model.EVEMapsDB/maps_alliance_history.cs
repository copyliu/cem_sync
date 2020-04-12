using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_alliance_history
    {
        public maps_alliance_history()
        {
            maps_eventhistory = new HashSet<maps_eventhistory>();
        }

        [Key]
        public int id { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime date { get; set; }
        public long alliance_id { get; set; }
        public long? old_execorp_id { get; set; }
        public long? new_execorp_id { get; set; }

        [ForeignKey(nameof(alliance_id))]
        [InverseProperty(nameof(maps_alliances.maps_alliance_history))]
        public virtual maps_alliances alliance_ { get; set; }
        [ForeignKey(nameof(new_execorp_id))]
        [InverseProperty(nameof(maps_corporation.maps_alliance_historynew_execorp_))]
        public virtual maps_corporation new_execorp_ { get; set; }
        [ForeignKey(nameof(old_execorp_id))]
        [InverseProperty(nameof(maps_corporation.maps_alliance_historyold_execorp_))]
        public virtual maps_corporation old_execorp_ { get; set; }
        [InverseProperty("all_his_")]
        public virtual ICollection<maps_eventhistory> maps_eventhistory { get; set; }
    }
}
