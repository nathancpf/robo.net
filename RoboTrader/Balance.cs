using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RoboTrader
{
    [DataContract]
    public class Saldo
    {

        [DataMember]
        public double BRL { get; set; }

        [DataMember]
        public double BRLReal { get; set; }

        [DataMember]
        public double BTC { get; set; }

        [DataMember]
        public double OrderBuyPending { get; set; }

        [DataMember]
        public double WithdrawalBRLPending { get; set; }

        [DataMember]
        public double WithdrawalBTCPending { get; set; }

        [DataMember]
        public double OrderSellBTCPending { get; set; }
      
    }

    public class Balance
    {
        public string success { get; set; }
        public List<Saldo> oReturn { get; set; }
        public string date { get; set; }
        public string timestamp { get; set; }
    }

}
