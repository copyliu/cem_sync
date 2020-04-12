using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVEMarketSite.Model
{
    [Table("constellations")]
    public partial class constellations
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public long constellationid { get; set; }
        [Required]
        public string constellationname { get; set; }
        public long faction { get; set; }
        public long regionid { get; set; }
    }
}
