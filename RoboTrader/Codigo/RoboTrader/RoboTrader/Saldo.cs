using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RoboTrader
{
    [DataContract]
    public class SaldoB2U
    {
        [DataMember]
        public decimal saldoBRL { get; set; }

        [DataMember]
        public decimal saldoBTC { get; set; }
    }
}
