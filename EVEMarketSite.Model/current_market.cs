using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace EVEMarketSite.Model
{
    [Table("current_market")]
    public partial class current_market
    {
        private int _minvolume;
        private int _volenter;
        private int _volremain;
        private Instant _issued;
        private int _range;
        private int _interval;
        private double _price;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long regionid { get; set; }
        public long? systemid { get; set; }
        public long stationid { get; set; }
        public int typeid { get; set; }
        public int bid { get; set; }

        public double price
        {
            get => _price;
            set
            {
                if (Math.Abs(_price - value) > 0.00001)
                {
                    this.reportedtime = Instant.FromDateTimeOffset(DateTimeOffset.Now);
                }
                _price = value;
            }
        }

        public long orderid { get; set; }

        public int minvolume
        {
            get => _minvolume;
            set
            {
                if (_minvolume != value)
                {
                    this.reportedtime = Instant.FromDateTimeOffset(DateTimeOffset.Now);
                }
                _minvolume = value;
            }
        }

        public int volremain
        {
            get => _volremain;
            set
            {
                if (_volremain != value)
                {
                    this.reportedtime = Instant.FromDateTimeOffset(DateTimeOffset.Now);
                }
                _volremain = value;
            }
        }

        public int volenter
        {
            get => _volenter;
            set
            {
                if (_volenter != value)
                {
                    this.reportedtime = Instant.FromDateTimeOffset(DateTimeOffset.Now);
                }
                _volenter = value;
            }
        }

        [Column(TypeName = "timestamp(6) with time zone")]
        public Instant issued
        {
            get => _issued;
            set
            {
                if (_issued != value)
                {
                    this.reportedtime = Instant.FromDateTimeOffset(DateTimeOffset.Now);
                }
                _issued = value;
            }
        }

        public int range
        {
            get => _range;
            set
            {
                if (_range != value)
                {
                    this.reportedtime = Instant.FromDateTimeOffset(DateTimeOffset.Now);
                }
                _range = value;
            }
        }

        public long reportedby { get; set; }
        [Column(TypeName = "timestamp(6) with time zone")]
        public Instant reportedtime { get; set; }
        public int source { get; set; }

        public int interval
        {
            get => _interval;
            set
            {
                if (_interval != value)
                {
                    this.reportedtime = Instant.FromDateTimeOffset(DateTimeOffset.Now);
                }
                _interval = value;
            }
        }

        [ForeignKey(nameof(regionid))]
        [InverseProperty(nameof(regions.current_market_p001))]
        public virtual regions region { get; set; }
        [ForeignKey(nameof(systemid))]
        [InverseProperty(nameof(systems.current_market_p001))]
        public virtual systems system { get; set; }
        [ForeignKey(nameof(typeid))]
        [InverseProperty(nameof(evetypes.current_market_p001))]
        public virtual evetypes type { get; set; }
        [ForeignKey(nameof(stationid))]
        [InverseProperty(nameof(stations.current_markets))]
        public virtual stations station { get; set; }
    }
}
