using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVEMarketSite.Model
{
    [Table("regions")]
    public partial class regions
    {
        public regions()
        {
          
            current_market_p001 = new HashSet<current_market>();
           
            market_markethistory = new HashSet<market_markethistory>();
           
            market_realtimehistory_201710 = new HashSet<market_realtimehistory>();
           
            systems=new HashSet<systems>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public long regionid { get; set; }
        [Required]
        public string regionname { get; set; }


        [InverseProperty("region")]
        public virtual ICollection<systems> systems { get; set; }
        [InverseProperty("region")]
        public virtual ICollection<current_market> current_market_p001 { get; set; }
       
        [InverseProperty("region")]
        public virtual ICollection<market_markethistory> market_markethistory { get; set; }

        [InverseProperty("region")]
        public virtual ICollection<market_markethistorybyday> market_markethistorybyday { get; set; }

        [InverseProperty("region")]
        public virtual ICollection<market_realtimehistory> market_realtimehistory_201710 { get; set; }
       
    }
}
