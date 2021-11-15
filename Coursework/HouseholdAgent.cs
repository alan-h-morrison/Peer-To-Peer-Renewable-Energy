using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActressMas;

namespace Coursework
{
    public enum HouseType { Buy, Sell };
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
        private int profitGain;
        private int profitLoss;

        private HouseType type;
        private HousePosition position;

        public HouseholdAgent(HousePosition housePosition)
        {
            position = housePosition;
            profitGain = 0;
            profitLoss = 0;
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

        private void HandleStart(string details)
        {

            string[] values = details.Split(' ');

            demand = Int32.Parse(values[0]);
            generation = Int32.Parse(values[1]);
            utilityBuy = Int32.Parse(values[2]);
            utilitySell = Int32.Parse(values[3]);
            energyDiff = generation - demand;

            // Price to buy renewable is lower than the price to buy utility
            renewableBuy = rand.Next((utilityBuy - 3), utilityBuy);

            // Price to sell renewable is higher than the price to sell to utility
            renewableSell = rand.Next(utilitySell, (utilitySell + 10));

            // Decides whether a household is a buyer, seller or does not need extra energy
            if (energyDiff > 0)
            {
                // Console.WriteLine($"{Name}: Sell {energyDiff}");
                type = HouseType.Sell;
                Send("community", $"register sell {renewableSell} {energyDiff}");
            }
            else if (energyDiff < 0)
            {
                // Make negative number positive
                energyDiff = energyDiff * -1;

                //  Console.WriteLine($"{Name}: Buy {energyDiff}");
                type = HouseType.Buy;
                Send("community", $"register buy {renewableBuy} {energyDiff}");
            }
            else
            {
                Console.WriteLine($"{Name}: Met Energy Demand");
                Stop();
            }
            //Console.WriteLine($"{Name}: \n\tdemand = {demand}\n\tgeneration = {generation}\n\tBuy Utility = {utilityBuy}\n\tBuy Renewable = {renewableBuy}\n\tSell Utility = {utilitySell}\n\tSell Renewable = {renewableSell}");
        }
        private void HandleSold()
        {

        }

        private void HandleBought()
        {

        }

        private void HandleNoBuyers()
        {
            Console.WriteLine($"{Name}: Buy {energyDiff}");

            for (int i = 1; i < energyDiff; i++)
            {
                profitGain = profitGain + utilitySell;
            }
            Console.WriteLine($"{Name}: profit gain = {profitGain}");
            Stop();
        }

        private void HandleNoSellers()
        {
            Console.WriteLine($"{Name}: Sell {energyDiff}");

            for (int i = 1; i < energyDiff; i++)
            {
                profitLoss = profitLoss + utilityBuy;
            }
            Console.WriteLine($"{Name}: profit loss = {profitLoss}");
            Stop();
        }
    }
}
