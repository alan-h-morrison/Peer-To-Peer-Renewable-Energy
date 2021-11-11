﻿using System;
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

        private HouseType type;
        private HousePosition position;

        public HouseholdAgent(HousePosition housePosition)
        {
            position = housePosition;
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

                    case "Buyer":
                        HandleNoBuyers();
                        break;

                    case "Seller":
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
                Send("community", $"register sell {energyDiff} {renewableSell}");
            }
            else if (energyDiff < 0)
            {
                // Make negative number positive
                energyDiff = energyDiff * -1;

                //  Console.WriteLine($"{Name}: Buy {energyDiff}");
                type = HouseType.Buy;
                Send("community", $"register buy {energyDiff}");
            }
            else
            {
                Console.WriteLine($"{Name}: Met Energy Demand");
                Stop();
            }
            //Console.WriteLine($"{Name}: \n\tdemand = {demand}\n\tgeneration = {generation}\n\tBuy Utility = {utilityBuy}\n\tBuy Renewable = {renewableBuy}\n\tSell Utility = {utilitySell}\n\tSell Renewable = {renewableSell}");
        }

        private void HandleNoBuyers()
        {
            Console.WriteLine($"{Name}: Buy {energyDiff}");

            for (int i = 1; i < energyDiff; i++)
            {
                //Console.WriteLine($"{Name}: Buy Utility");
            }
        }

        private void HandleNoSellers()
        {
            Console.WriteLine($"{Name}: Sell {energyDiff}");

            for (int i = 1; i < energyDiff; i++)
            {
                //Console.WriteLine($"{Name}: Sell Utility");
            }
        }
    }
}