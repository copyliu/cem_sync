using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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

        public static bool operator ==(CrestOrder order, CurrentMarket model)
        {
            if (order == null && model == null) return true;
            if (order == null || model == null) return false;
            
            if (order.id != model.orderid) return false;
            if (order.stationID != model.stationid) return false;
            if (order.type != model.typeid) return false;
            if (order.buy != (model.bid == 1)) return false;
            if (order.price != model.price) return false;
            if (order.volume != model.volremain) return false;
            if (order.minVolume != model.minvolume) return false;
            if (order.volumeEntered != model.volenter) return false;
            if (order.issued != model.issued) return false;
            if (order.duration != model.interval) return false;
            if (Helpers.ConvertRange(order.range) != model.range) return false;
            return true;
        }

        public static bool operator !=(CrestOrder order, CurrentMarket model)
        {
            return !(order == model);
        }
    }
}
