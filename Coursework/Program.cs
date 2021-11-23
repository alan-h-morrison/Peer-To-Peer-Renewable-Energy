using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActressMas;

namespace Coursework
{
    class Program
    {
        static void Main(string[] args)
        {
            var env = new EnvironmentMas();
            int counter = 1;

            for (int i = 1; i <= Settings.positiveHouseholds; i++)
            {
                var householdAgent = new HouseholdAgent(HousePosition.Positive);
                env.Add(householdAgent, $"household{counter:D2}");
                counter++;
            }

            for (int i = 1; i <= Settings.neutralHouseholds; i++)
            {
                var householdAgent = new HouseholdAgent(HousePosition.Neutral);
                env.Add(householdAgent, $"household{counter:D2}");
                counter++;
            }

            for (int i = 1; i <= Settings.negativeHouseholds; i++)
            {
                var householdAgent = new HouseholdAgent(HousePosition.Negative);
                env.Add(householdAgent, $"household{counter:D2}");
                counter++;
            }

            var enviromentAgent = new EnvironmentAgent();
            env.Add(enviromentAgent, "enviroment");

            var communityAgent = new CommunityManager();
            env.Add(communityAgent, "community");

            env.Start();

            Console.WriteLine("\n============================================================");
            Console.WriteLine("SYSTEM RESULTS");
            Console.WriteLine("============================================================");
            Console.WriteLine($"Total Messages = {Settings.Counter}");

            Console.ReadLine();
        }
    }
}
