using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CEMSync.Model.EVEMapsDB
{
    public partial class maps_stastationtypes
    {
        public maps_stastationtypes()
        {
            maps_outposts = new HashSet<maps_outposts>();
        }

        [Key]
        public int stationTypeID { get; set; }
        public double? dockEntryX { get; set; }
        public double? dockEntryY { get; set; }
        public double? dockEntryZ { get; set; }
        public double? dockOrientationX { get; set; }
        public double? dockOrientationY { get; set; }
        public double? dockOrientationZ { get; set; }
        public short? operationID { get; set; }
        public short? officeSlots { get; set; }
        public double? reprocessingEfficiency { get; set; }
        public short? conquerable { get; set; }
        [Required]
        [StringLength(100)]
        public string name { get; set; }

        [InverseProperty("stationType_")]
        public virtual ICollection<maps_outposts> maps_outposts { get; set; }
    }
}
