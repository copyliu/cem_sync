using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EVEMarketSite.Model
{
    // public class dogma_attributes
    // {
    //     [Key]
    //     public int attribute_id { get; set; }
    //     public string name { get; set; }
    //     public string description { get; set; }
    //     public double? default_value { get; set; }
    //     public string display_name { get; set; }
    //     public bool? high_is_good { get; set; }
    //     public int? icon_id { get; set; }
    //     public bool? published { get; set; }
    //     public bool? stackable { get; set; }
    //     public string unit_id { get; set; }
    // }
    // public class type_attributes
    // {
    //     
    //     public int attribute_id { get; set; }
    //     public int type_id { get; set; }
    //     public double value { get; set; }
    //     [ForeignKey(nameof(attribute_id))]
    //     public virtual dogma_attributes dogma_attribute { get; set; }
    //
    //     [ForeignKey(nameof(type_id))]
    //     [InverseProperty(nameof(evetypes.attributes))]
    //     public virtual evetypes evetype { get; set; }
    // }
}
