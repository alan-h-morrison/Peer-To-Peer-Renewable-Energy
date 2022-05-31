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
            var env = new EnvironmentMas(noTurns:100);

            // used to add appropriate household number
            int counter = 1;

            // creates and adds all the total positive households to the environment
            for (int i = 1; i <= Settings.positiveHouseholds; i++)
            {
                var householdAgent = new HouseholdAgent(HousePosition.Positive);
                env.Add(householdAgent, $"household{counter:D2}");
                counter++;
            }

            // creates and adds all the total neutral households to the environment
            for (int i = 1; i <= Settings.neutralHouseholds; i++)
            {
                var householdAgent = new HouseholdAgent(HousePosition.Neutral);
                env.Add(householdAgent, $"household{counter:D2}");
                counter++;
            }

            // creates and adds all the total negative households to the environment
            for (int i = 1; i <= Settings.negativeHouseholds; i++)
            {
                var householdAgent = new HouseholdAgent(HousePosition.Negative);
                env.Add(householdAgent, $"household{counter:D2}");
                counter++;
            }

            // adds environment agent to the environment
            var environmentAgent = new EnvironmentAgent();
            env.Add(environmentAgent, "environment");

            // adds community manager agent to the environment
            var communityAgent = new CommunityManager();
            env.Add(communityAgent, "community");

            env.Start();

            // Prints the total amount of messages passed running the system to screen
            Console.WriteLine("\n============================================================");
            Console.WriteLine("SYSTEM RESULTS");
            Console.WriteLine("============================================================");
            Console.WriteLine($"total messages = {Settings.Counter}");

            Console.WriteLine("\nPress Enter To Exit...");
            Console.ReadLine();
        }
    }
}
