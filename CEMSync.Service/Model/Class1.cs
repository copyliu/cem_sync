using System;
using System.Collections.Generic;
using System.Text;

namespace CEMSync.Service.Model
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
}
