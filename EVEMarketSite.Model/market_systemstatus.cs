using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVEMarketSite.Model
{
    [Table("market_systemstatus")]
    public partial class market_systemstatus
    {
        [Key]
        public int id { get; set; }
        [Required]
        [StringLength(255)]
        public string name { get; set; }
        [Required]
        public string value { get; set; }
    }
}
