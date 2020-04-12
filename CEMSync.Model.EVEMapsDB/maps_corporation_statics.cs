using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_corporation_statics
    {
        [Key]
        public int id { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime date { get; set; }
        public long corporation_id { get; set; }
        public int membercount { get; set; }

        [ForeignKey(nameof(corporation_id))]
        [InverseProperty(nameof(maps_corporation.maps_corporation_statics))]
        public virtual maps_corporation corporation_ { get; set; }
    }
}
