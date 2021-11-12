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

            for (int i = 1; i <= Settings.NumHouseholds; i++)
            {
                var householdAgent = new HouseholdAgent(HousePosition.Neutral);
                env.Add(householdAgent, $"household{i:D2}");
            }

            var enviromentAgent = new EnvironmentAgent();
            env.Add(enviromentAgent, "enviroment");

            var communityAgent = new CommunityManager();
            env.Add(communityAgent, "community");

            env.Start();

            Console.WriteLine("hello");
            Console.ReadLine();
        }
    }
}
