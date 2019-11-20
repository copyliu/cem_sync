using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using cem_updater_core.DAL;

namespace cem_updater_core.Model
{

    public  class ESIMarketOrder
    {
        protected bool Equals(ESIMarketOrder other)
        {
            return order_id == other.order_id && type_id == other.type_id && location_id == other.location_id && system_id == other.system_id && volume_total == other.volume_total && volume_remain == other.volume_remain && min_volume == other.min_volume && price.Equals(other.price) && is_buy_order == other.is_buy_order && duration == other.duration && issued.Equals(other.issued) && string.Equals(range, other.range);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ESIMarketOrder) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = order_id.GetHashCode();
                hashCode = (hashCode * 397) ^ type_id;
                hashCode = (hashCode * 397) ^ location_id.GetHashCode();
                hashCode = (hashCode * 397) ^ system_id;
                hashCode = (hashCode * 397) ^ volume_total;
                hashCode = (hashCode * 397) ^ volume_remain;
                hashCode = (hashCode * 397) ^ min_volume;
                hashCode = (hashCode * 397) ^ price.GetHashCode();
                hashCode = (hashCode * 397) ^ is_buy_order.GetHashCode();
                hashCode = (hashCode * 397) ^ duration;
                hashCode = (hashCode * 397) ^ issued.GetHashCode();
                hashCode = (hashCode * 397) ^ (range != null ? range.GetHashCode() : 0);
                return hashCode;
            }
        }

        public long order_id { get; set; }
        public int type_id { get; set; }
        public long location_id { get; set; }
        public int system_id { get; set; }
        public int volume_total { get; set; }
        public int volume_remain { get; set; }
        public int min_volume { get; set; }
        public double price { get; set; }
        public bool is_buy_order { get; set; }
        public int duration { get; set; }
        public System.DateTime issued { get; set; }
        public string range { get; set; }
        [JsonIgnore] public int regionid { get; set; }

        public static bool operator ==(ESIMarketOrder order, CurrentMarket model)
        {
            if (ReferenceEquals(order, null) && ReferenceEquals(model, null)) return true;
            if (ReferenceEquals(order, null) || ReferenceEquals(model, null)) return false;

            if (order.order_id != model.orderid) return false;
            if (order.location_id != model.stationid) return false;
            if (order.type_id != model.typeid) return false;
            if (order.is_buy_order != (model.bid == 1)) return false;
            if (Math.Abs(order.price - model.price) > 0.0001) return false;
            if (order.volume_remain != model.volremain) return false;
            if (order.min_volume != model.minvolume) return false;
            if (order.volume_total != model.volenter) return false;
            if (order.issued.ToUniversalTime() != model.issued.ToUniversalTime()) return false;
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
        public long? systemid;
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
        protected bool Equals(CrestOrder other)
        {
            return buy == other.buy && issued.Equals(other.issued) && price.Equals(other.price) &&
                   volume == other.volume && duration == other.duration && id == other.id &&
                   minVolume == other.minVolume && volumeEntered == other.volumeEntered &&
                   string.Equals(range, other.range) && stationID == other.stationID && type == other.type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CrestOrder) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = buy.GetHashCode();
                hashCode = (hashCode * 397) ^ issued.GetHashCode();
                hashCode = (hashCode * 397) ^ price.GetHashCode();
                hashCode = (hashCode * 397) ^ volume;
                hashCode = (hashCode * 397) ^ duration;
                hashCode = (hashCode * 397) ^ id.GetHashCode();
                hashCode = (hashCode * 397) ^ minVolume;
                hashCode = (hashCode * 397) ^ volumeEntered;
                hashCode = (hashCode * 397) ^ (range != null ? range.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ stationID.GetHashCode();
                hashCode = (hashCode * 397) ^ type;
                return hashCode;
            }
        }

        public bool buy { get; set; }
        public DateTime issued { get; set; }
        public double price { get; set; }
        public int volume { get; set; }
        public int duration { get; set; }
        public long id { get; set; }
        public int minVolume { get; set; }
        public int volumeEntered { get; set; }
        public string range { get; set; }
        public long stationID { get; set; }
        public int type { get; set; }

        [JsonIgnore] public int regionid { get; set; }
        [JsonIgnore] public int? systemid { get; set; }

        public static bool operator ==(CrestOrder order, CurrentMarket model)
        {
            if (ReferenceEquals(order, null) && ReferenceEquals(model, null)) return true;
            if (ReferenceEquals(order, null) || ReferenceEquals(model, null)) return false;

            if (order.id != model.orderid) return false;
            if (order.stationID != model.stationid) return false;
            if (order.type != model.typeid) return false;
            if (order.buy != (model.bid == 1)) return false;
            if (Math.Abs(order.price - model.price) > 0.0001) return false;
            if (order.volume != model.volremain) return false;
            if (order.minVolume != model.minvolume) return false;
            if (order.volumeEntered != model.volenter) return false;
            if (order.issued.ToUniversalTime() != model.issued.ToUniversalTime()) return false;
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
