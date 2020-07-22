using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace EVEMarketSite.Model
{
    [Table("contracts_info")]
    public partial class contracts_info
    {
        public contracts_info()
        {
            contracts_items = new HashSet<contracts_items>();
        }

        [Key]
        public int ID { get; set; }
        [Column(TypeName = "numeric(30,4)")]
        public decimal? buyout { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public Instant date_issued { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public Instant? date_expired { get; set; }
        public int? issuer_corporation_id { get; set; }
        public int? issuer_id { get; set; }
        [StringLength(255)]
        public string type { get; set; }
        public long region_id { get; set; }
        [Required]
        [StringLength(255)]
        public string title { get; set; }
        [Column(TypeName = "numeric(30,4)")]
        public decimal? price { get; set; }
        public long? start_location_id { get; set; }
        public long? end_location_id { get; set; }
        public bool? for_corporation { get; set; }
        public bool vaild { get; set; }
        public bool otheritem { get; set; }
        [InverseProperty("contract_")]
        public virtual ICollection<contracts_items> contracts_items { get; set; }
    }
}
