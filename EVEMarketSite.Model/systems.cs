using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVEMarketSite.Model
{
    public partial class systems
    {
        public systems()
        {
          
            current_market_p001 = new HashSet<current_market>();
           
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public long systemid { get; set; }
        [Required]
        public string systemname { get; set; }
        public long regionid { get; set; }
        public long faction { get; set; }
        public double security { get; set; }
        public long constellationid { get; set; }
        public double? truesec { get; set; }
        [ForeignKey(nameof(regionid))]
        [InverseProperty(nameof(regions.systems))]
        public virtual regions region { get; set; }

        [InverseProperty("system")]
        public virtual ICollection<current_market> current_market_p001 { get; set; }




    }
}
