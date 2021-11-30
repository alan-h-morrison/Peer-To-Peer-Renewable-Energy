using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework
{
    public class Bid
    {
        public string Bidder { get; set; }
        public int BidValue { get; set; }
        public int EnergyDifference { get; set; }

        public Bid(string bidder, int bidValue, int difference)
        {
            Bidder = bidder;
            BidValue = bidValue;
            EnergyDifference = difference;
        }

        public void DecreaseEnergy()
        {
            this.EnergyDifference = EnergyDifference - 1;
        }
    }
}
