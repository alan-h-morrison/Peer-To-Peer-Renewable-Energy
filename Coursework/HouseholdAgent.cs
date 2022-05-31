using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActressMas;

namespace Coursework
{
    public enum HousePosition { Positive, Neutral, Negative }

    class HouseholdAgent : Agent
    {
        private Random rand = new Random();

        private int demand;
        private int generation;
        private int utilityBuy;
        private int utilitySell;
        private int renewableBuy;
        private int renewableSell;
        private int energyDiff;

        private bool start;

        private HousePosition position;
        private string type;

        private int profit;
        private int utilityCounter;
        private int renewableCounter;

        public HouseholdAgent(HousePosition housePosition)
        {
            position = housePosition;
            profit = 0;
            utilityCounter = 0;
            renewableCounter = 0;
        }

        // Every household agent sends the message "start" to the environment agent
        public override void Setup()
        {
            Console.WriteLine($"Starting [{Name}]");

            // global message counter incremented
            Settings.Increment();
            Send("environment", "start");
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");

                message.Parse(out string action, out string parameters);

                switch (action)
                {
                    case "inform":
                        HandleStart(parameters);
                        break;

                    case "sold":
                        HandleSold(parameters);
                        break;

                    case "bought":
                        HandleBought(parameters);
                        break;

                    case "no-buyers":
                        HandleNoBuyers();
                        break;

                    case "no-sellers":
                        HandleNoSellers();
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override void ActDefault()
        {
           // if a household has participated in the market and has no energy left to buy/sell
           if(energyDiff == 0 && start == true)
           {
                Console.WriteLine($"[{Name}]: Met Energy Demand");

                // reset energy difference to send to environment for final statistical analysis
                energyDiff = demand - generation;

                // make energy difference positive if negative
                energyDiff = Math.Abs(energyDiff);

                // global message counter incremented
                Settings.Increment();

                // send environment agent variables for final statistical analysis of the system
                Send("environment", $"finish {Name} {type} {profit} {energyDiff} {utilityCounter} {renewableCounter} {utilityBuy} {utilitySell}");
                Stop();
           }
        }

        // Method called to process infromation send by the environment agent
        // register as either buyer/seller/absent household
        private void HandleStart(string details)
        {
            string[] values = details.Split(' ');

            demand = Int32.Parse(values[0]);
            generation = Int32.Parse(values[1]);
            utilityBuy = Int32.Parse(values[2]);
            utilitySell = Int32.Parse(values[3]);
            energyDiff = generation - demand;

            // household's position on renewable energy is used to determine their action-selection function when bidding buying or selling renewable energy
            if (position == HousePosition.Positive)
            {
                renewableBuy = rand.Next(utilityBuy - 6, utilityBuy - 2);
                renewableSell = rand.Next(utilitySell + 2, utilitySell + 8);
            }

            if (position == HousePosition.Neutral)
            {
                renewableBuy = utilityBuy - 6;
                renewableSell = utilitySell + 8;
            }

            if (position == HousePosition.Negative)
            {
                renewableBuy = utilityBuy - 10;
                renewableSell = utilitySell + 15;

            }

            // households are either buyers, sellers or absent depending on whether their energy difference is positive, negative or is equal to zero
            if (energyDiff > 0) 
            {
                // household's type set to seller
                type = "seller";

                // global message counter incremented
                Settings.Increment();

                // messages community manager to register their bid as a seller, their price to sell renewable energy and their energy difference
                Send("community", $"bid sell {renewableSell} {energyDiff}");

                // household has begun participating in the market
                start = true;
            }
            else if (energyDiff < 0)
            {
                // makes energy difference is positive
                energyDiff = Math.Abs(energyDiff);

                // household's type set to buyer
                type = "buyer";

                // global message counter incremented
                Settings.Increment();

                // messages community manager to register their bid as a buyer, their price to buy renewable energy and their energy difference
                Send("community", $"bid buy {renewableBuy} {energyDiff}");

                // household has begun participating in the market
                start = true;
            }
            else
            {
                // household does not have excess or deficit in energy, they've met their energy demand and will not participat in the market
                type = "n/a";
                Console.WriteLine($"[{Name}]: Met Energy Demand");

                // global message counter incremented
                Settings.Increment();

                // send environment agent variables for final statistical analysis of the system
                Send("environment", $"finish {Name} {type} {profit} {energyDiff} {utilityCounter} {renewableCounter} {utilityBuy} {utilitySell}");
                Stop();
            }
        }

        // an energy lot was sold, the energy difference decrements, renewable counter increments and amount sold is added to a household's profit
        private void HandleSold(string amount)
        {
            energyDiff = energyDiff - 1;
            renewableCounter++;
            profit = profit + Convert.ToInt32(amount);
        }

        // an energy lot was bought, the energy difference decrements, renewable counter increments and amount bought is take from a household's profit
        private void HandleBought(string amount)
        {
            energyDiff = energyDiff - 1;
            renewableCounter++;
            profit = profit - Convert.ToInt32(amount);
        }

        // no more buyers present, all remaining energy for a household has is sold to their utility company
        private void HandleNoBuyers()
        {
            Console.WriteLine($"[{Name}]: Sell remaining {energyDiff} kWh to utility company");

            for (int i = energyDiff; i > 0; i--)
            {
                profit = profit + utilitySell;
                utilityCounter++;
            }

            // reset energy difference to send to environment for final statistical analysis
            energyDiff = demand - generation;

            // make energy difference positive if negative
            energyDiff = Math.Abs(energyDiff);

            // global message counter incremented
            Settings.Increment();

            // send environment agent variables for final statistical analysis of the system
            Send("environment", $"finish {Name} {type} {profit} {energyDiff} {utilityCounter} {renewableCounter} {utilityBuy} {utilitySell}");
            Stop();
        }

        // no more sellers present, all remaining energy for a household has is bought from their utility company
        private void HandleNoSellers()
        {
            Console.WriteLine($"[{Name}]: Buy remaining {energyDiff} kWh from utility company");

            for (int i = energyDiff; i > 0; i--)
            {
                profit = profit -  utilityBuy;
                utilityCounter++;
            }

            // reset energy difference to send to environment for final statistical analysis
            energyDiff = demand - generation;

            // make energy difference positive if negative
            energyDiff = Math.Abs(energyDiff);

            // global message counter incremented
            Settings.Increment();

            // send environment agent variables for final statistical analysis of the system
            Send("environment", $"finish {Name} {type} {profit} {energyDiff} {utilityCounter} {renewableCounter} {utilityBuy} {utilitySell}");
            Stop();
        }
    }
}
