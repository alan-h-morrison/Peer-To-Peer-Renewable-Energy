using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework
{
    public class Settings
    {
        public static int positiveHouseholds = 20;
        public static int neutralHouseholds = 0;
        public static int negativeHouseholds = 0;
        public static int totalHouseholds = positiveHouseholds + neutralHouseholds + negativeHouseholds;
        public int totalProfit = 0;
    }
}
