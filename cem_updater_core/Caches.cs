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
        }
    }
}
