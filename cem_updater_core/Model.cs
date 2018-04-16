using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace cem_updater_core
{

    public  class ESIMarketOrder
    {
        public long order_id;
        public int type_id;
        public long location_id;
        public int system_id;
        public int volume_total;
        public int volume_remain;
        public int min_volume;
        public double price;
        public bool is_buy_order;
        public int duration;
        public System.DateTime issued;
        public string range;
        public static bool operator ==(ESIMarketOrder order, CurrentMarket model)
        {
            if (ReferenceEquals(order, null) && ReferenceEquals(model, null)) return true;
            if (ReferenceEquals(order, null) || ReferenceEquals(model, null)) return false;

            if (order.order_id != model.orderid) return false;
            if (order.location_id != model.stationid) return false;
            if (order.type_id != model.typeid) return false;
            if (order.is_buy_order != (model.bid == 1)) return false;
            if (order.price != model.price) return false;
            if (order.volume_remain != model.volremain) return false;
            if (order.min_volume != model.minvolume) return false;
            if (order.volume_total != model.volenter) return false;
            if (order.issued != model.issued) return false;
            if (order.duration != model.interval) return false;
            if (Helpers.ConvertRange(order.range) != model.range) return false;
            return true;
        }

        public static bool operator !=(ESIMarketOrder order, CurrentMarket model)
        {
            return !(order == model);
        }
    }


    public class CrestMarketResult
    {
        public class Next
        {
            public string href;
        }
        public int totalCount;
        public int pageCount;
        public List<CrestOrder> items;
        public Next next;
    }

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
            if (ReferenceEquals(order, null) && ReferenceEquals(model, null)) return true;
            if (ReferenceEquals(order, null) || ReferenceEquals(model, null)) return false;

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
