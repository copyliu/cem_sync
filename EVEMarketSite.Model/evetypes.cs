﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVEMarketSite.Model
{
    [Table("invMetaTypes")]
    public class invMetaTypes
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int typeID { get; set; }
        public int? parentTypeID { get; set; }
        public int? metaGroupID { get; set; }

        [ForeignKey(nameof(typeID))]
        [InverseProperty(nameof(evetypes.invmetatype))]
        public virtual evetypes type { get; set; }

        [ForeignKey(nameof(parentTypeID))]
        [InverseProperty(nameof(evetypes.invmetatype_parent))]
        public virtual evetypes parenttype { get; set; }
    }



    [Table("evetypes")]
    public partial class evetypes
    {
        public evetypes()
        {
          
            current_market_p001 = new HashSet<current_market>();
           
            market_markethistory = new HashSet<market_markethistory>();
           
            market_realtimehistory = new HashSet<market_realtimehistory>();
            attributes=new HashSet<type_attributes>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int typeID { get; set; }
        public int? groupID { get; set; }
        [Required]
        [StringLength(300)]
        public string typeName { get; set; }
        public string description { get; set; }
        public double? mass { get; set; }
        public double? volume { get; set; }
        public double? capacity { get; set; }
        public int? portionSize { get; set; }
        public int? raceID { get; set; }
        [Column(TypeName = "numeric(21,4)")]
        public decimal? basePrice { get; set; }
        public bool? published { get; set; }
        public int? marketGroupID { get; set; }
        public int? iconID { get; set; }
        [Required]
        [StringLength(300)]
        public string typeName_en { get; set; }
        [StringLength(9000)]
        public string description_en { get; set; }
     
        [InverseProperty(nameof(type_attributes.evetype))]
        public virtual ICollection<type_attributes> attributes { get; set; }

        [InverseProperty("type")]
        public virtual ICollection<current_market> current_market_p001 { get; set; }
       
        [InverseProperty("type")]
        public virtual ICollection<market_markethistory> market_markethistory { get; set; }

        [InverseProperty("type")]
        public virtual ICollection<market_markethistorybyday> market_markethistorybyday { get; set; }

        [InverseProperty("type")]
        public virtual ICollection<market_realtimehistory> market_realtimehistory { get; set; }

        [InverseProperty(nameof(invMetaTypes.type))]
        public virtual ICollection<invMetaTypes> invmetatype { get; set; }
        [InverseProperty(nameof(invMetaTypes.parenttype))]
        public virtual ICollection<invMetaTypes> invmetatype_parent { get; set; }

    }
}
