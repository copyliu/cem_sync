using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVEMarketSite.Model
{
    [Table("contracts_itemattr")]
    public partial class contracts_itemattr
    {
        [Key]
        public long item_id { get; set; }
        [Key]
        public int attribute_id { get; set; }
        public double value { get; set; }

        [ForeignKey(nameof(attribute_id))]
        [InverseProperty(nameof(dogma_attributes.contracts_itemattrs))]
        public virtual dogma_attributes attributes { get; set; }
        [ForeignKey(nameof(item_id))]
        [InverseProperty(nameof(contracts_itemdata.contracts_itemattr))]
        public virtual contracts_itemdata item_ { get; set; }
    }
}
