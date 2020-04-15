using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace EVEMarketSite.Model
{
    [Table("current_market")]
    public partial class current_market
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long regionid { get; set; }
        public long? systemid { get; set; }
        public long stationid { get; set; }
        public int typeid { get; set; }
        public int bid { get; set; }
        public double price { get; set; }
        public long orderid { get; set; }
        public int minvolume { get; set; }
        public int volremain { get; set; }
        public int volenter { get; set; }
        [Column(TypeName = "timestamp(6) with time zone")]
        public Instant issued { get; set; }
        public int range { get; set; }
        public long reportedby { get; set; }
        [Column(TypeName = "timestamp(6) with time zone")]
        public Instant reportedtime { get; set; }
        public int source { get; set; }
        public int interval { get; set; }

        [ForeignKey(nameof(regionid))]
        [InverseProperty(nameof(regions.current_market_p001))]
        public virtual regions region { get; set; }
        [ForeignKey(nameof(systemid))]
        [InverseProperty(nameof(systems.current_market_p001))]
        public virtual systems system { get; set; }
        [ForeignKey(nameof(typeid))]
        [InverseProperty(nameof(evetypes.current_market_p001))]
        public virtual evetypes type { get; set; }
        [ForeignKey(nameof(stationid))]
        [InverseProperty(nameof(stations.current_market_p001))]
        public virtual stations station { get; set; }
    }
}
