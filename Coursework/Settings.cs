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
        public static int positiveHouseholds = 30;
        public static int neutralHouseholds = 30;
        public static int negativeHouseholds = 30;

        public static int totalHouseholds = positiveHouseholds + neutralHouseholds + negativeHouseholds;

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
