using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_serverstatus
    {
        [Key]
        public int id { get; set; }
        public int online { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime dataTime { get; set; }
    }
}
