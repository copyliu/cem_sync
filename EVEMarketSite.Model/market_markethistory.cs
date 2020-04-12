using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace EVEMarketSite.Model
{
    public partial class market_markethistory
    {
        [Key]
        public int id { get; set; }
        public long regionid { get; set; }
        public int typeid { get; set; }
        public long orderCount { get; set; }
        public double lowPrice { get; set; }
        public double highPrice { get; set; }
        public double avgPrice { get; set; }
        public long volume { get; set; }
        [Column(TypeName = "date")]
        public DateTime date { get; set; }
        
        [ForeignKey(nameof(regionid))]
        [InverseProperty(nameof(regions.market_markethistory))]
        public virtual regions region { get; set; }
        [ForeignKey(nameof(typeid))]
        [InverseProperty(nameof(evetypes.market_markethistory))]
        public virtual evetypes type { get; set; }
    }
}
