using System;
using System.Collections.Generic;
using System.Text;

namespace cem_updater_core.Model
{
    public class RedisQ
    {
        public PizzaKM package { get; set; }
    }
    public class PizzaKM
    {
        public int killID { get; set; }
        public class PizzaZkb
        {
            public string hash { get; set; }
        }

        public PizzaZkb zkb { get; set; }
    }
    public class Esi_war_kms
    {
        public int killmail_id { get; set; }
        public string killmail_hash { get; set; }
    }

    public  class Kb_waiting_api
    {
        public int killID;
        public string hash;
        public bool? error;
        public string traceback;
        public bool? fromapi;

    }

    public class Kb_war
    {
        public int warID;
        public string rawdata;
        public bool? finished;
        public int? lastkm;
        public DateTime? cacheutil;

    }
}
