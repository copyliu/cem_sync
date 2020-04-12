using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_corporation_history
    {
        public maps_corporation_history()
        {
            maps_eventhistory = new HashSet<maps_eventhistory>();
        }

        [Key]
        public int id { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime changedatetime { get; set; }
        public long corporation_id { get; set; }
        public long? old_Alliance_id { get; set; }
        public long? new_Alliance_id { get; set; }
        public long? old_ceoID { get; set; }
        [StringLength(40)]
        public string old_ceoName { get; set; }
        public long? new_ceoID { get; set; }
        [StringLength(40)]
        public string new_ceoName { get; set; }
        public double? old_taxRate { get; set; }
        public double? new_taxRate { get; set; }
        public long? old_shares { get; set; }
        public long? new_shares { get; set; }

        [ForeignKey(nameof(corporation_id))]
        [InverseProperty(nameof(maps_corporation.maps_corporation_history))]
        public virtual maps_corporation corporation_ { get; set; }
        [ForeignKey(nameof(new_Alliance_id))]
        [InverseProperty(nameof(maps_alliances.maps_corporation_historynew_Alliance_))]
        public virtual maps_alliances new_Alliance_ { get; set; }
        [ForeignKey(nameof(old_Alliance_id))]
        [InverseProperty(nameof(maps_alliances.maps_corporation_historyold_Alliance_))]
        public virtual maps_alliances old_Alliance_ { get; set; }
        [InverseProperty("corp_his_")]
        public virtual ICollection<maps_eventhistory> maps_eventhistory { get; set; }
    }
}
