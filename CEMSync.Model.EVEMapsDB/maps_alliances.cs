using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_alliances
    {
        public maps_alliances()
        {
            maps_alliance_history = new HashSet<maps_alliance_history>();
            maps_alliance_statics = new HashSet<maps_alliance_statics>();
            maps_campaignatk = new HashSet<maps_campaignatk>();
            maps_campaigndef = new HashSet<maps_campaigndef>();
            maps_corporation = new HashSet<maps_corporation>();
            maps_corporation_historynew_Alliance_ = new HashSet<maps_corporation_history>();
            maps_corporation_historyold_Alliance_ = new HashSet<maps_corporation_history>();
            maps_sovereignty = new HashSet<maps_sovereignty>();
            maps_sovereignty_historyfromalliance_ = new HashSet<maps_sovereignty_history>();
            maps_sovereignty_historytoalliance_ = new HashSet<maps_sovereignty_history>();
            maps_struct = new HashSet<maps_struct>();
        }

        [Key]
        public long allianceID { get; set; }
        [Required]
        [StringLength(100)]
        public string name { get; set; }
        [Required]
        [StringLength(50)]
        public string shortName { get; set; }
        public long? executorCorp_id { get; set; }
        public int memberCount { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime startDate { get; set; }

        [ForeignKey(nameof(executorCorp_id))]
        [InverseProperty("maps_alliances")]
        public virtual maps_corporation executorCorp_ { get; set; }
        [InverseProperty("alliance_")]
        public virtual ICollection<maps_alliance_history> maps_alliance_history { get; set; }
        [InverseProperty("alliance_")]
        public virtual ICollection<maps_alliance_statics> maps_alliance_statics { get; set; }
        [InverseProperty("alliance_")]
        public virtual ICollection<maps_campaignatk> maps_campaignatk { get; set; }
        [InverseProperty("alliance_")]
        public virtual ICollection<maps_campaigndef> maps_campaigndef { get; set; }
        [InverseProperty("Alliance_")]
        public virtual ICollection<maps_corporation> maps_corporation { get; set; }
        [InverseProperty(nameof(maps_corporation_history.new_Alliance_))]
        public virtual ICollection<maps_corporation_history> maps_corporation_historynew_Alliance_ { get; set; }
        [InverseProperty(nameof(maps_corporation_history.old_Alliance_))]
        public virtual ICollection<maps_corporation_history> maps_corporation_historyold_Alliance_ { get; set; }
        [InverseProperty("alliance_")]
        public virtual ICollection<maps_sovereignty> maps_sovereignty { get; set; }
        [InverseProperty(nameof(maps_sovereignty_history.fromalliance_))]
        public virtual ICollection<maps_sovereignty_history> maps_sovereignty_historyfromalliance_ { get; set; }
        [InverseProperty(nameof(maps_sovereignty_history.toalliance_))]
        public virtual ICollection<maps_sovereignty_history> maps_sovereignty_historytoalliance_ { get; set; }
        [InverseProperty("alliance_")]
        public virtual ICollection<maps_struct> maps_struct { get; set; }
    }
}
