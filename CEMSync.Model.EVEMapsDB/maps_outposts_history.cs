using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_outposts_history
    {
        public maps_outposts_history()
        {
            maps_eventhistory = new HashSet<maps_eventhistory>();
        }

        [Key]
        public int id { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime changedatetime { get; set; }
        public long station_id { get; set; }
        [StringLength(100)]
        public string old_stationName { get; set; }
        public long? old_corporation_id { get; set; }
        [StringLength(100)]
        public string new_stationName { get; set; }
        public long? new_corporation_id { get; set; }

        [ForeignKey(nameof(new_corporation_id))]
        [InverseProperty(nameof(maps_corporation.maps_outposts_historynew_corporation_))]
        public virtual maps_corporation new_corporation_ { get; set; }
        [ForeignKey(nameof(old_corporation_id))]
        [InverseProperty(nameof(maps_corporation.maps_outposts_historyold_corporation_))]
        public virtual maps_corporation old_corporation_ { get; set; }
        [ForeignKey(nameof(station_id))]
        [InverseProperty(nameof(maps_outposts.maps_outposts_history))]
        public virtual maps_outposts station_ { get; set; }
        [InverseProperty("out_his_")]
        public virtual ICollection<maps_eventhistory> maps_eventhistory { get; set; }
    }
}
