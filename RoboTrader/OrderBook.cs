using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboTrader
{
    public class OrderBook
    {

        public List<List<double>> asks { get; set; }
        public List<List<double>> bids { get; set; }

    }
}
