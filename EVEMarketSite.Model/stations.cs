using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVEMarketSite.Model
{
    [Table("stations")]
    public partial class stations
    {
        public stations()
        {
            // this.current_markets=new HashSet<current_market>();
        }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public long stationid { get; set; }
        [Required]
        public string stationname { get; set; }
       
        public int systemid { get; set; }
        public long corpid { get; set; }
        public int? typeid { get; set; }

        [InverseProperty(nameof(current_market.station))]
        public virtual ICollection<current_market> current_markets { get; set; }
    }
}
