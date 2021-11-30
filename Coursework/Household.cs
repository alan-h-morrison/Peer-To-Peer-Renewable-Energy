using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework
{
    public class Household
    {
        public string HouseName { get; set; }
        public string HouseType { get; set; }
        public int HouseProfit { get; set; }
        public int EnergyDifference { get; set; }
        public int UtilityCounter { get; set; }
        public int RenwableCounter { get; set; }
        public int utilityBuy { get; set; }
        public int utilitySell { get; set; }

        public Household(string name, string type, int profit, int diff, int utilCounter, int resCounter, int buy, int sell)
        {
            HouseName = name;
            HouseType = type;
            HouseProfit = profit;
            EnergyDifference = diff;
            UtilityCounter = utilCounter;
            RenwableCounter = resCounter;
            utilityBuy = buy;
            utilitySell = sell;
        }
    }

}
