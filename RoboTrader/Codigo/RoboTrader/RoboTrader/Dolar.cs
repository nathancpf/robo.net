using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboTrader
{
    

    public class Dolar
    {
        public double BRL { get; set; }
    }

    public class RootObjectDolar
    {
        public string @base { get; set; }
        public string date { get; set; }
        public Dolar rates { get; set; }
    }
}
