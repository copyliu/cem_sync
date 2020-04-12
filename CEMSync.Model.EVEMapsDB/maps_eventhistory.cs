using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_eventhistory
    {
        [Key]
        public int id { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime date { get; set; }
        public int? all_his_id { get; set; }
        public int? corp_his_id { get; set; }
        public int? sov_his_id { get; set; }
        public int? out_his_id { get; set; }
        public int? cam_his_id { get; set; }
        public int? stu_his_id { get; set; }

        [ForeignKey(nameof(all_his_id))]
        [InverseProperty(nameof(maps_alliance_history.maps_eventhistory))]
        public virtual maps_alliance_history all_his_ { get; set; }
        [ForeignKey(nameof(cam_his_id))]
        [InverseProperty(nameof(maps_campaigns_history.maps_eventhistory))]
        public virtual maps_campaigns_history cam_his_ { get; set; }
        [ForeignKey(nameof(corp_his_id))]
        [InverseProperty(nameof(maps_corporation_history.maps_eventhistory))]
        public virtual maps_corporation_history corp_his_ { get; set; }
        [ForeignKey(nameof(out_his_id))]
        [InverseProperty(nameof(maps_outposts_history.maps_eventhistory))]
        public virtual maps_outposts_history out_his_ { get; set; }
        [ForeignKey(nameof(sov_his_id))]
        [InverseProperty(nameof(maps_sovereignty_history.maps_eventhistory))]
        public virtual maps_sovereignty_history sov_his_ { get; set; }
        [ForeignKey(nameof(stu_his_id))]
        [InverseProperty(nameof(maps_struct_history.maps_eventhistory))]
        public virtual maps_struct_history stu_his_ { get; set; }
    }
}
