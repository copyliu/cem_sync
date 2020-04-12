using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_sovereignty_history
    {
        public maps_sovereignty_history()
        {
            maps_eventhistory = new HashSet<maps_eventhistory>();
        }

        [Key]
        public int id { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime changedatetime { get; set; }
        public long? fromalliance_id { get; set; }
        public long? toalliance_id { get; set; }
        public int? fromfaction_id { get; set; }
        public int? tofaction_id { get; set; }
        public long? fromcorp_id { get; set; }
        public long? tocorp_id { get; set; }
        public int system_id { get; set; }

        [ForeignKey(nameof(fromalliance_id))]
        [InverseProperty(nameof(maps_alliances.maps_sovereignty_historyfromalliance_))]
        public virtual maps_alliances fromalliance_ { get; set; }
        [ForeignKey(nameof(fromcorp_id))]
        [InverseProperty(nameof(maps_corporation.maps_sovereignty_historyfromcorp_))]
        public virtual maps_corporation fromcorp_ { get; set; }
        [ForeignKey(nameof(fromfaction_id))]
        [InverseProperty(nameof(maps_factions.maps_sovereignty_historyfromfaction_))]
        public virtual maps_factions fromfaction_ { get; set; }
        [ForeignKey(nameof(system_id))]
        [InverseProperty(nameof(maps_systems.maps_sovereignty_history))]
        public virtual maps_systems system_ { get; set; }
        [ForeignKey(nameof(toalliance_id))]
        [InverseProperty(nameof(maps_alliances.maps_sovereignty_historytoalliance_))]
        public virtual maps_alliances toalliance_ { get; set; }
        [ForeignKey(nameof(tocorp_id))]
        [InverseProperty(nameof(maps_corporation.maps_sovereignty_historytocorp_))]
        public virtual maps_corporation tocorp_ { get; set; }
        [ForeignKey(nameof(tofaction_id))]
        [InverseProperty(nameof(maps_factions.maps_sovereignty_historytofaction_))]
        public virtual maps_factions tofaction_ { get; set; }
        [InverseProperty("sov_his_")]
        public virtual ICollection<maps_eventhistory> maps_eventhistory { get; set; }
    }
}
