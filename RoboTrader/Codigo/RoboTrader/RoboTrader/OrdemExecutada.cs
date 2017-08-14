using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RoboTrader
{

    [DataContract]
    public class Ordem
    {
        [DataMember]
        public string asset { get; set; }
        [DataMember]
        public string currency { get; set; }
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string action { get; set; }
        [DataMember]
        public string status { get; set; }
        [DataMember]

        public string price { get; set; }
        [DataMember]
        public string amount { get; set; }
        [DataMember]
        public string executedPriceAverage { get; set; }
        [DataMember]
        public string executedAmount { get; set; }
        [DataMember]
        public string dateCreated { get; set; }
        [DataMember]
        public string tipoRobo { get; set; }

        [DataMember]
        public bool ordemPai { get; set; }

        public bool removida { get; set; }

        [DataMember]
        public double precoLimite { get; set; }

        public double obterPreco()
        {
            return Convert.ToDouble(price);
        }

        public double obterQuantidade()
        {
            return Convert.ToDouble(amount);
        }

        public double obterQuantidadeExecutada()
        {
            return Convert.ToDouble(executedAmount);
        }

        public void definirQuantidade(double valor)
        {
            this.amount = Convert.ToString(valor);
        }
    }

    [DataContract]
    public class OrderList
    {
        [DataMember]
        public string success { get; set; }
        [DataMember]
        public List<Ordem> oReturn { get; set; }
        [DataMember]
        public string date { get; set; }
        [DataMember]
        public string timestamp { get; set; }
    }
}
