using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVEMarketSite.Model
{
    public partial class marketgroup
    {
        public marketgroup()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int marketGroupID { get; set; }
        [Required]
        public string marketGroupName { get; set; }
        public string description { get; set; }
        public int? iconID { get; set; }
        public bool? hasTypes { get; set; }
        [Required]
        [StringLength(300)]
        public string marketGroupName_en { get; set; }
        [StringLength(9000)]
        public string description_en { get; set; }
        public int? parentGroupID { get; set; }

    }
}
