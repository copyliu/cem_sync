using System;
using System.Collections.Generic;
using System.Text;

namespace cem_updater_core
{
    public class CurrentMarket
    {
        public long id;
        public long regionid;
        public long systemid;
        public long stationid;
        public int typeid;
        public int bid;
        public double price;
        public long orderid;
        public int minvolume;
        public int volremain;
        public int volenter;
        public DateTime issued;
        public int range;
        public long reportedby;
        public DateTime reportedtime;
        public int source;
        public int interval;
    }

    public class CrestOrder
    {
        public bool buy;
        public DateTime issued;
        public double price;
        public int volume;
        public int duration;
        public long id;
        public int minVolume;
        public int volumeEntered;
        public string range;
        public long stationID;
        public int type;
    }
}
