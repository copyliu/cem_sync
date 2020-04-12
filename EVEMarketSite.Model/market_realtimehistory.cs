using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace EVEMarketSite.Model
{
    public partial class market_realtimehistory
    {
        [Key]
        public long id { get; set; }
        public long regionid { get; set; }
        public int typeid { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public Instant date { get; set; }
        public double sell { get; set; }
        public double buy { get; set; }
        public double sellvol { get; set; }
        public double buyvol { get; set; }

        [ForeignKey(nameof(regionid))]
        [InverseProperty(nameof(regions.market_realtimehistory_201710))]
        public virtual regions region { get; set; }
        [ForeignKey(nameof(typeid))]
        [InverseProperty(nameof(evetypes.market_realtimehistory))]
        public virtual evetypes type { get; set; }
    }
}
