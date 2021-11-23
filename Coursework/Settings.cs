using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Coursework
{
    public class Settings
    {
        public static int positiveHouseholds = 12;
        public static int neutralHouseholds = 0;
        public static int negativeHouseholds = 0;
        public static int totalHouseholds = positiveHouseholds + neutralHouseholds + negativeHouseholds;
        public int totalProfit = 0;

        private static int counter = 0;
        private static readonly object lockObject = new object();
        public static void Increment()
        {
            lock (lockObject)
            {
                counter++;
            }
        }

        public static int Counter
        {
            get
            {
                lock (lockObject)
                {
                    return counter;
                }
            }
        }
    }
}
