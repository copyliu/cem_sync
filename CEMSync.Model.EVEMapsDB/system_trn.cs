using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class system_trn
    {
        [Key]
        public int id { get; set; }
        public int systemID { get; set; }
        [Required]
        [StringLength(150)]
        public string text { get; set; }
    }
}
