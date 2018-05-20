﻿using System;
using System.Collections.Generic;
using System.Text;

namespace cem_updater_core.Model
{
    public class RedisQ
    {
        public PizzaKM package;
    }
    public class PizzaKM
    {
        public int killID;
        public class PizzaZkb
        {
            public string hash;
        }

        public PizzaZkb zkb;
    }
    public class Esi_war_kms
    {
        public int killmail_id;
        public string killmail_hash;
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
