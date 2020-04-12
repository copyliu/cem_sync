using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace EVEMarketSite.Model
{
    public partial class market_markethistorybyday
    {
        [Key]
        public int id { get; set; }

        [Column(TypeName = "date")]
        public NodaTime.LocalDate date { get; set; }
        public double min { get; set; }
        public double max { get; set; }
        public double start { get; set; }
        public double end { get; set; }
        public long volume { get; set; }
        public long regionid { get; set; }
        public int typeid { get; set; }
        public long order { get; set; }

        [ForeignKey(nameof(regionid))]
        [InverseProperty(nameof(regions.market_markethistorybyday))]
        public virtual regions region { get; set; }
        [ForeignKey(nameof(typeid))]
        [InverseProperty(nameof(evetypes.market_markethistorybyday))]
        public virtual evetypes type { get; set; }
    }
}