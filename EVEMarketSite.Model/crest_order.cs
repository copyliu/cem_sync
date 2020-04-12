using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVEMarketSite.Model
{
    public partial class crest_order
    {
        public bool? buy { get; set; }
        [Column(TypeName = "character varying")]
        public string issued { get; set; }
        public double? price { get; set; }
        public int? volumeEntered { get; set; }
        public long? stationID { get; set; }
        public int? volume { get; set; }
        [Column(TypeName = "character varying")]
        public string range { get; set; }
        public int? minVolume { get; set; }
        public int? duration { get; set; }
        public long? type { get; set; }
        public long? id { get; set; }
    }
}
