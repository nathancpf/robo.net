using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RoboTrader
{
    [DataContract]
    public class Ticker
    {
        [DataMember]
        public string high { get; set; }
        [DataMember]
        public string low { get; set; }
        [DataMember]
        public string vol { get; set; }
        [DataMember]
        public string last { get; set; }
        [DataMember]
        public string buy { get; set; }
        [DataMember]
        public string buyQty { get; set; }
        [DataMember]
        public string sell { get; set; }
        
        [DataMember]
        public string sellQty { get; set; }
        [DataMember]
        public string date { get; set; }
    }

    public class RootObjectTicker
    {
        public Ticker ticker { get; set; }
    }
}
