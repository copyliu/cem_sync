using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_corporation
    {
        public maps_corporation()
        {
            maps_alliance_historynew_execorp_ = new HashSet<maps_alliance_history>();
            maps_alliance_historyold_execorp_ = new HashSet<maps_alliance_history>();
            maps_alliances = new HashSet<maps_alliances>();
            maps_corporation_history = new HashSet<maps_corporation_history>();
            maps_corporation_statics = new HashSet<maps_corporation_statics>();
            maps_outposts = new HashSet<maps_outposts>();
            maps_outposts_historynew_corporation_ = new HashSet<maps_outposts_history>();
            maps_outposts_historyold_corporation_ = new HashSet<maps_outposts_history>();
            maps_sovereignty = new HashSet<maps_sovereignty>();
            maps_sovereignty_historyfromcorp_ = new HashSet<maps_sovereignty_history>();
            maps_sovereignty_historytocorp_ = new HashSet<maps_sovereignty_history>();
        }

        [Key]
        public long corporationID { get; set; }
        public long? Alliance_id { get; set; }
        [Required]
        [StringLength(100)]
        public string corporationName { get; set; }
        [Required]
        [StringLength(10)]
        public string ticker { get; set; }
        public long ceoID { get; set; }
        [Required]
        [StringLength(40)]
        public string ceoName { get; set; }
        public long stationID { get; set; }
        [Required]
        [StringLength(100)]
        public string stationName { get; set; }
        public double taxRate { get; set; }
        public int memberCount { get; set; }
        public long shares { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        [StringLength(200)]
        public string url { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime AlliancestartDate { get; set; }
        public bool flag { get; set; }

        [ForeignKey(nameof(Alliance_id))]
        [InverseProperty("maps_corporation")]
        public virtual maps_alliances Alliance_ { get; set; }
        [InverseProperty(nameof(maps_alliance_history.new_execorp_))]
        public virtual ICollection<maps_alliance_history> maps_alliance_historynew_execorp_ { get; set; }
        [InverseProperty(nameof(maps_alliance_history.old_execorp_))]
        public virtual ICollection<maps_alliance_history> maps_alliance_historyold_execorp_ { get; set; }
        [InverseProperty("executorCorp_")]
        public virtual ICollection<maps_alliances> maps_alliances { get; set; }
        [InverseProperty("corporation_")]
        public virtual ICollection<maps_corporation_history> maps_corporation_history { get; set; }
        [InverseProperty("corporation_")]
        public virtual ICollection<maps_corporation_statics> maps_corporation_statics { get; set; }
        [InverseProperty("corporation_")]
        public virtual ICollection<maps_outposts> maps_outposts { get; set; }
        [InverseProperty(nameof(maps_outposts_history.new_corporation_))]
        public virtual ICollection<maps_outposts_history> maps_outposts_historynew_corporation_ { get; set; }
        [InverseProperty(nameof(maps_outposts_history.old_corporation_))]
        public virtual ICollection<maps_outposts_history> maps_outposts_historyold_corporation_ { get; set; }
        [InverseProperty("corporation_")]
        public virtual ICollection<maps_sovereignty> maps_sovereignty { get; set; }
        [InverseProperty(nameof(maps_sovereignty_history.fromcorp_))]
        public virtual ICollection<maps_sovereignty_history> maps_sovereignty_historyfromcorp_ { get; set; }
        [InverseProperty(nameof(maps_sovereignty_history.tocorp_))]
        public virtual ICollection<maps_sovereignty_history> maps_sovereignty_historytocorp_ { get; set; }
    }
}
