using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVEMarketSite.Model
{
    [Table("contracts_itemdata")]
    public partial class contracts_itemdata
    {
        public contracts_itemdata()
        {
            contracts_itemattr = new HashSet<contracts_itemattr>();
            contracts_items = new HashSet<contracts_items>();
        }

        [Key]
        public long item_id { get; set; }
        public int type_id { get; set; }
        public int? source_type_id { get; set; }
        public int? mutator_type_id { get; set; }
        public int? created_by { get; set; }

        [InverseProperty("item_")]
        public virtual ICollection<contracts_itemattr> contracts_itemattr { get; set; }
        [InverseProperty("item_")]
        public virtual ICollection<contracts_items> contracts_items { get; set; }
    }
}
