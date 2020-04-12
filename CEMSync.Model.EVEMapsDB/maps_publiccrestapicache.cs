using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_publiccrestapicache
    {
        [Key]
        public int id { get; set; }
        [Required]
        [StringLength(50)]
        public string name { get; set; }
        [Required]
        [StringLength(500)]
        public string url { get; set; }
        [Column(TypeName = "jsonb")]
        public string data { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime? cache_util { get; set; }
    }
}
