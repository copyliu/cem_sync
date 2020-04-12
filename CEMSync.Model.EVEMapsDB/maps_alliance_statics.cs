using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_alliance_statics
    {
        [Key]
        public int id { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime date { get; set; }
        public long alliance_id { get; set; }
        public int membercount { get; set; }
        public int corpcount { get; set; }
        public int sovers0 { get; set; }
        public int sovers1 { get; set; }
        public int sovers2 { get; set; }
        public int sovers3 { get; set; }
        public int sovers4 { get; set; }
        public int sovers5 { get; set; }
        public int outposts { get; set; }

        [ForeignKey(nameof(alliance_id))]
        [InverseProperty(nameof(maps_alliances.maps_alliance_statics))]
        public virtual maps_alliances alliance_ { get; set; }
    }
}
