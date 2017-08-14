using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RoboTrader
{
    [DataContract]
    public class Parametros
    {
        [DataMember]
        public decimal margemLucro = 1M;

        [DataMember]
        public decimal quantidadeMaxima = 1M;

        [DataMember]
        public decimal spreadCompra = 1M;

        [DataMember]
        public decimal spreadVenda = 5M;

        [DataMember]
        public decimal quantidadeOrdem = 0.01M;

        [DataMember]
        public decimal variacaoPermitida = 0.1M;

        [DataMember]
        public double minimoLucroBRL = 10;

        [DataMember]
        public double incrementoOrdem = 1;

        [DataMember]
        public double spreadCompraVenda = 20;

        [DataMember]
        public string chaveAPI = "";//key

        [DataMember]
        public string chaveSecreta = "";//secret
        
        public double obterQuantidadeOrdem()
        {
            return Convert.ToDouble(quantidadeOrdem);
        }
            
    }
}
