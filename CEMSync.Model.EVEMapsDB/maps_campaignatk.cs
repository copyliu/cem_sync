using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_campaignatk
    {
        [Key]
        public int id { get; set; }
        public long campaigns_id { get; set; }
        public long? alliance_id { get; set; }
        public double score { get; set; }

        [ForeignKey(nameof(alliance_id))]
        [InverseProperty(nameof(maps_alliances.maps_campaignatk))]
        public virtual maps_alliances alliance_ { get; set; }
        [ForeignKey(nameof(campaigns_id))]
        [InverseProperty(nameof(maps_campaigns.maps_campaignatk))]
        public virtual maps_campaigns campaigns_ { get; set; }
    }
}
