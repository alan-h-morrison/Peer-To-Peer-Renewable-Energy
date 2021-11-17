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
        private int profit;

        private bool start;

        private HousePosition position;
        private string type;

        public HouseholdAgent(HousePosition housePosition)
        {
            position = housePosition;
            profit = 0;
        }

        public override void Setup()
        {
            Console.WriteLine($"Starting [{Name}]");

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
                Send("enviroment", $"finish {profit}");
                Console.WriteLine($"[{Name}] ({type}) profit = {profit}");
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
                renewableBuy = rand.Next((utilityBuy - 3), utilityBuy);

                // Price to sell renewable is higher than the price to sell to utility
                renewableSell = rand.Next(utilitySell, (utilitySell + 10));
            }

            if (position == HousePosition.Neutral)
            {
                // Price to buy renewable is lower than the price to buy utility
                renewableBuy = rand.Next((utilityBuy - 3), utilityBuy);

                // Price to sell renewable is higher than the price to sell to utility
                renewableSell = rand.Next(utilitySell, (utilitySell + 10));
            }

            if (position == HousePosition.Negative)
            {
                // Price to buy renewable is lower than the price to buy utility
                renewableBuy = rand.Next((utilityBuy - 3), utilityBuy);

                // Price to sell renewable is higher than the price to sell to utility
                renewableSell = rand.Next(utilitySell, (utilitySell + 10));
            }

            // Decides whether a household is a buyer, seller or does not need extra energy
            if (energyDiff > 0)
            {
                // Console.WriteLine($"{Name}: Sell {energyDiff}");
                type = "seller";
                Send("community", $"register sell {renewableSell} {energyDiff}");
                start = true;
            }
            else if (energyDiff < 0)
            {
                // Make negative number positive
                energyDiff = energyDiff * -1;

                //  Console.WriteLine($"{Name}: Buy {energyDiff}");
                type = "buyer";
                Send("community", $"register buy {renewableBuy} {energyDiff}");
                start = true;
            }
            else
            {
                Console.WriteLine($"{Name} : Met Energy Demand");
                Send("enviroment", $"finish {profit}");
                Stop();
            }
            //Console.WriteLine($"{Name}: \n\tdemand = {demand}\n\tgeneration = {generation}\n\tBuy Utility = {utilityBuy}\n\tBuy Renewable = {renewableBuy}\n\tSell Utility = {utilitySell}\n\tSell Renewable = {renewableSell}");
        }
        private void HandleSold(string amount)
        {
            energyDiff = energyDiff - 1;
            profit = profit + Convert.ToInt32(amount);
        }

        private void HandleBought(string amount)
        {
            energyDiff = energyDiff - 1;
            profit = profit - Convert.ToInt32(amount);
        }

        private void HandleNoBuyers()
        {
            Console.WriteLine($"{Name}: Sell {energyDiff}");

            for (int i = energyDiff; i >= 0; i--)
            {
                profit = profit + utilitySell;
            }
            Console.WriteLine($"{Name} ({type}): profit = {profit}");
            Send("enviroment", $"finish {profit}");
            Stop();
        }

        private void HandleNoSellers()
        {
            Console.WriteLine($"{Name}: Buy {energyDiff}");

            for (int i = energyDiff; i >= 0; i--)
            {
                profit = profit -  utilityBuy;
            }
            Console.WriteLine($"{Name} ({type}): profit = {profit}");
            Send("enviroment", $"finish {profit}");
            Stop();
        }
    }
}
