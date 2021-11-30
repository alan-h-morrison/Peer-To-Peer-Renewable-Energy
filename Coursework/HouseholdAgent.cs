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

        public override void Setup()
        {
            Console.WriteLine($"Starting [{Name}]");

            Settings.Increment();
            Send("enviroment", "start");
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
           if(energyDiff == 0 && start == true)
           {
                // reset energy difference to send to enviroment for final statistical analysis
                energyDiff = demand - generation;

                energyDiff = Math.Abs(energyDiff);

                Settings.Increment();
                Send("enviroment", $"finish {Name} {type} {profit} {energyDiff} {utilityCounter} {renewableCounter} {utilityBuy} {utilitySell}");
                Stop();
           }
        }

        private void HandleStart(string details)
        {

            string[] values = details.Split(' ');

            demand = Int32.Parse(values[0]);
            generation = Int32.Parse(values[1]);
            utilityBuy = Int32.Parse(values[2]);
            utilitySell = Int32.Parse(values[3]);
            energyDiff = generation - demand;

            if(position == HousePosition.Positive)
            {
                // Price to buy renewable is lower than the price to buy utility
                renewableBuy = utilityBuy + 15;

                // Price to sell renewable is higher than the price to sell to utility
                renewableSell = utilitySell;

            }

            if (position == HousePosition.Neutral)
            {
                // Price to buy renewable is lower than the price to buy utility
                renewableBuy = utilityBuy - 5;

                // Price to sell renewable is higher than the price to sell to utility
                renewableSell = utilitySell + 10;

            }

            if (position == HousePosition.Negative)
            {
                // Price to buy renewable is lower than the price to buy utility
                renewableBuy = utilityBuy - 10;

                // Price to sell renewable is higher than the price to sell to utility
                renewableSell = utilitySell + 15;

            }

            // Decides whether a household is a buyer, seller or does not need extra energy
            if (energyDiff > 0)
            {
                // Console.WriteLine($"{Name}: Sell {energyDiff}");
                type = "seller";

                Settings.Increment();
                Send("community", $"register sell {renewableSell} {energyDiff}");
                start = true;
            }
            else if (energyDiff < 0)
            {
                // Make negative number positive
                energyDiff = Math.Abs(energyDiff);

                //  Console.WriteLine($"{Name}: Buy {energyDiff}");
                type = "buyer";

                Settings.Increment();
                Send("community", $"register buy {renewableBuy} {energyDiff}");
                start = true;
            }
            else
            {
                type = "n/a";
                Console.WriteLine($"{Name} : Met Energy Demand");

                Settings.Increment();
                Send("enviroment", $"finish {Name} {type} {profit} {energyDiff} {utilityCounter} {renewableCounter} {utilityBuy} {utilitySell}");
                Stop();
            }
            //Console.WriteLine($"{Name}: \n\tdemand = {demand}\n\tgeneration = {generation}\n\tBuy Utility = {utilityBuy}\n\tBuy Renewable = {renewableBuy}\n\tSell Utility = {utilitySell}\n\tSell Renewable = {renewableSell}");
        }
        private void HandleSold(string amount)
        {
            energyDiff = energyDiff - 1;
            renewableCounter++;
            profit = profit + Convert.ToInt32(amount);
        }

        private void HandleBought(string amount)
        {
            energyDiff = energyDiff - 1;
            renewableCounter++;
            profit = profit - Convert.ToInt32(amount);
        }

        private void HandleNoBuyers()
        {
            Console.WriteLine($"{Name}: Sell {energyDiff}");

            for (int i = energyDiff; i > 0; i--)
            {
                profit = profit + utilitySell;
                utilityCounter++;
            }
            //Console.WriteLine($"{Name} ({type}): profit = {profit}");

            energyDiff = demand - generation;

            energyDiff = Math.Abs(energyDiff);

            Settings.Increment();
            Send("enviroment", $"finish {Name} {type} {profit} {energyDiff} {utilityCounter} {renewableCounter} {utilityBuy} {utilitySell}");
            Stop();
        }

        private void HandleNoSellers()
        {
            Console.WriteLine($"{Name}: Buy {energyDiff}");

            for (int i = energyDiff; i > 0; i--)
            {
                profit = profit -  utilityBuy;
                utilityCounter++;
            }

            energyDiff = demand - generation;

            energyDiff = Math.Abs(energyDiff);
            //Console.WriteLine($"{Name} ({type}): profit = {profit}");

            Settings.Increment();      
            Send("enviroment", $"finish {Name} {type} {profit} {energyDiff} {utilityCounter} {renewableCounter} {utilityBuy} {utilitySell}");
            Stop();
        }
    }
}
