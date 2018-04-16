using System;
using System.Collections.Generic;
using System.Text;

namespace cem_updater_core
{
    public  class Caches
    {
        private static Dictionary<long, long> _stationSystemDictCn;
        private static Dictionary<long, long> _stationRegionDictCn;

        public static Dictionary<long, long> StationSystemDictCn
        {
            get
            {
                if (_stationSystemDictCn == null || _stationSystemDictCn.Count == 0)
                {
                    UpdateCaches();
                }
                return _stationSystemDictCn;
            }
        }

        public static Dictionary<long, long> StationRegionDictCn    
        {
            get
            {
                if (_stationRegionDictCn == null || _stationRegionDictCn.Count == 0)
                {
                    UpdateCaches();
                }
                return _stationRegionDictCn;
            }

        }

        public static void UpdateCaches()
        {
            var v = DAL.GetStations();
            _stationSystemDictCn = v[0];
            _stationRegionDictCn = v[1];
            var w = DAL.GetStations(true);
            _stationSystemDictTq = w[0];
            _stationRegionDictTq = w[1];
        }


        private static Dictionary<long, long> _stationSystemDictTq;
        private static Dictionary<long, long> _stationRegionDictTq;

        public static Dictionary<long, long> StationSystemDictTq
        {
            get
            {
                if (_stationSystemDictTq == null || _stationSystemDictTq.Count == 0)
                {
                    UpdateCaches();
                }
                return _stationSystemDictTq;
            }
        }

        public static Dictionary<long, long> StationRegionDictTq
        {
            get
            {
                if (_stationRegionDictTq == null || _stationRegionDictTq.Count == 0)
                {
                    UpdateCaches();
                }
                return _stationRegionDictTq;
            }

        }

    }
}
