using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVEMarketSite.Model
{
    [Table("contracts_items")]
    public partial class contracts_items
    {
        [Key]
        public long ID { get; set; }
        public int type_id { get; set; }
        public int quantity { get; set; }
        public bool? is_included { get; set; }
        public int? contract_id { get; set; }
        public bool? is_blueprint_copy { get; set; }
        public int? material_efficiency { get; set; }
        public int? time_efficiency { get; set; }
        public int? runs { get; set; }
        public long item_id { get; set; }

        [ForeignKey(nameof(contract_id))]
        [InverseProperty(nameof(contracts_info.contracts_items))]
        public virtual contracts_info contract_ { get; set; }
        [ForeignKey(nameof(item_id))]
        [InverseProperty(nameof(contracts_itemdata.contracts_items))]
        public virtual contracts_itemdata item_ { get; set; }
    }
}
