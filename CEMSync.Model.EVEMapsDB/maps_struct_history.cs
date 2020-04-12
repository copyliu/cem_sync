using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_struct_history
    {
        public maps_struct_history()
        {
            maps_eventhistory = new HashSet<maps_eventhistory>();
        }

        [Key]
        public int id { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTime date { get; set; }
        public int flag { get; set; }
        public long struct_id { get; set; }

        [ForeignKey(nameof(struct_id))]
        [InverseProperty(nameof(maps_struct.maps_struct_history))]
        public virtual maps_struct struct_ { get; set; }
        [InverseProperty("stu_his_")]
        public virtual ICollection<maps_eventhistory> maps_eventhistory { get; set; }
    }
}
