using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVEMarketSite.Model
{
    [Table("stations")]
    public partial class stations
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public long stationid { get; set; }
        [Required]
        public string stationname { get; set; }
        public long systemid { get; set; }
        public long corpid { get; set; }
        [InverseProperty("station")]
        public virtual ICollection<current_market> current_market_p001 { get; set; }
    }
}
