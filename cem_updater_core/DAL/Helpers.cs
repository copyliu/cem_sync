using System;

namespace cem_updater_core.DAL
{
    public class Helpers
    {
        public static int ConvertRange(string range)
        {
            if (Int32.TryParse(range, out var r))
            {
                return r;
            }
            switch (range)
            {
                case "station": return 0;
                case "solarsystem": return 32767;
                case "region": return 65535;
                default: return 65535;
            }
        }

        public  static string GetConnString(bool tq = false)
        {
            return tq ? connectionstring_tq : connectionstring_cn;
        }

        public static string connectionstring_cn;
        public static string connectionstring_tq;
    }
}