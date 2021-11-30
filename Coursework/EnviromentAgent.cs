/*
 * Author: Simon Powers
 * An Environment Agent that sends information to a Household Agent
 * about that household's demand, generation, and prices to buy and sell
 * from the utility company, on that day. Responds whenever pinged
 * by a Household Agent with a "start" message.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ActressMas;
using Coursework;

class EnvironmentAgent : Agent
{
    private Random rand = new Random();

    private const int MinGeneration = 5; //min possible generation from renewable energy on a day for a household (in kWh)
    private const int MaxGeneration = 15; //max possible generation from renewable energy on a day for a household (in kWh)
    private const int MinDemand = 5; //min possible demand on a day for a household (in kWh)
    private const int MaxDemand = 15; //max possible demand on a day for a household (in kWh)
    private const int MinPriceToBuyFromUtility = 12; //min possible price to buy 1kWh from the utility company (in pence)
    private const int MaxPriceToBuyFromUtility = 22; //max possible price to buy 1kWh from the utility company (in pence)
    private const int MinPriceToSellToUtility = 2; //min possible price to sell 1kWh to the utility company (in pence)
    private const int MaxPriceToSellToUtility = 5; //max possible price to sell 1kWh to the utility company (in pence)

    private int activeHouseholds = Settings.totalHouseholds;

    private int totalProfit = 0;
    private int profitGain = 0;
    private int profitLoss = 0;

    private int utilityGain = 0;
    private int utilityLoss = 0;
    private int totalUtilityGain = 0;
    private int totalUtilityLoss = 0;
    private int totalUtilityProfit = 0;

    private int totalDemmand = 0;
    private int totalExcess = 0;
    private int totalRenewableBought = 0;
    private int totalRenewableSold = 0;
    private int totalUtilityBought = 0;
    private int totalUtilitySold = 0;

    private int numSeller = 0;
    private int numBuyer = 0;
    private int numNeither = 0;

    private class Household
    {
        public string HouseName { get; set; }
        public string HouseType { get; set; }
        public int HouseProfit { get; set; }
        public int EnergyDifference { get; set; }
        public int UtilityCounter { get; set; }
        public int RenwableCounter { get; set; }
        public int utilityBuy { get; set; }
        public int utilitySell { get; set; }

        public Household(string name, string type, int profit, int diff, int utilCounter, int resCounter, int buy, int sell)
        {
            HouseName = name;
            HouseType = type;
            HouseProfit = profit;
            EnergyDifference = diff;
            UtilityCounter = utilCounter;
            RenwableCounter = resCounter;
            utilityBuy = buy;
            utilitySell = sell;
        }
    }

    private List<Household> households;

    public EnvironmentAgent()
    {
        households = new List<Household>();
    }

    public override void Act(Message message)
    {
        Console.WriteLine($"\t{message.Format()}");

        message.Parse(out string action, out string parameters);

        switch (action)
        {

            case "start": //this agent only responds to "start" messages
                string senderID = message.Sender; //get the sender's name so we can reply to them
                int demand = rand.Next(MinDemand, MaxDemand); //the household's demand in kWh
                int generation = rand.Next(MinGeneration, MaxGeneration); //the household's demand in kWh
                int priceToBuyFromUtility = rand.Next(MinPriceToBuyFromUtility, MaxPriceToBuyFromUtility); //what the household's utility company
                                                                                                           //charges to buy 1kWh from it
                int priceToSellToUtility = rand.Next(MinPriceToSellToUtility, MaxPriceToSellToUtility);    //what the household's utility company
                                                                                                           //offers to buy 1kWh of renewable energy for
                string content = $"inform {demand} {generation} {priceToBuyFromUtility} {priceToSellToUtility}";

                Settings.Increment();
                Send(senderID, content); //send the message with this information back to the household agent that requested it
                break;

            case "finish":
                HandleFinish(parameters);
                break;

            default:
                break;
        }
    }

    private void HandleFinish(string info)
    {
        activeHouseholds--;

        string[] details = info.Split(' ');
        string name = details[0];
        string type = details[1];
        int profit = Convert.ToInt32(details[2]);
        int energyDiff = Convert.ToInt32(details[3]);
        int utilityCounter = Convert.ToInt32(details[4]);
        int renewableCounter = Convert.ToInt32(details[5]);
        int utilityBuy = Convert.ToInt32(details[6]);
        int utilitySell = Convert.ToInt32(details[7]);

        var bid = new Household(name, type, profit, energyDiff, utilityCounter, renewableCounter, utilityBuy, utilitySell);

        households.Add(bid);



        if (activeHouseholds == 0)
        {

            Thread.Sleep(100);
            Console.WriteLine("\n******* Day Finished *******");

            foreach (var houseItem in households)
            {

                totalProfit = totalProfit + houseItem.HouseProfit;
                

                // calculation to find out statistics for the whole system
                if(houseItem.HouseType.Contains("seller"))
                {
                    numSeller++;

                    utilityGain = houseItem.EnergyDifference * houseItem.utilitySell;
                    totalUtilityGain = totalUtilityGain + utilityGain;

                    profitGain = profitGain + houseItem.HouseProfit;

                    totalExcess = totalExcess + houseItem.EnergyDifference;
                    totalRenewableSold = totalRenewableSold + houseItem.RenwableCounter;
                    totalUtilitySold = totalUtilitySold + houseItem.UtilityCounter;
                    // print out stats for every individual household who participated in the system

                    Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------");
                    Console.WriteLine($"[{houseItem.HouseName}] ({houseItem.HouseType}): total profit = {houseItem.HouseProfit}, potential utility profit = {utilityGain}, energy diff = {houseItem.EnergyDifference},  renewable energy = {houseItem.RenwableCounter}, utility energy = {houseItem.UtilityCounter}");
                }

                if (houseItem.HouseType.Contains("buyer"))
                {
                    numBuyer++;

                    utilityLoss = houseItem.EnergyDifference * houseItem.utilityBuy;
                    utilityLoss = utilityLoss * -1;
                    totalUtilityLoss = totalUtilityLoss + utilityLoss;

                    profitLoss = profitLoss + houseItem.HouseProfit;

                    totalDemmand = totalDemmand + houseItem.EnergyDifference;
                    totalRenewableBought = totalRenewableBought + houseItem.RenwableCounter;
                    totalUtilityBought = totalUtilityBought + houseItem.UtilityCounter;

                    // print out stats for every individual household who participated in the system
                    Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------");
                    Console.WriteLine($"[{houseItem.HouseName}] ({houseItem.HouseType}): total profit = {houseItem.HouseProfit}, potential utility profit = {utilityLoss}, energy diff = {houseItem.EnergyDifference},  renewable energy = {houseItem.RenwableCounter}, utility energy = {houseItem.UtilityCounter}");
                }

                if(houseItem.HouseType.Contains("n/a"))
                {
                    numNeither++;
                }
                totalUtilityProfit = totalUtilityGain + totalUtilityLoss;
            }

            Console.WriteLine("\n============================================================");
            Console.WriteLine("HOUSEHOLDS BREAKDOWN");
            Console.WriteLine("============================================================");

            Console.WriteLine($"total households = {households.Count}");

            Console.WriteLine($"\nnumber of buyer household(s) = {numBuyer}");
            Console.WriteLine($"number of seller household(s) = {numSeller}");
            Console.WriteLine($"number of absent households(s) = {numNeither}");

            Console.WriteLine($"\nnumber of positive household(s) = {Settings.positiveHouseholds}");
            Console.WriteLine($"number of neutral household(s) = {Settings.neutralHouseholds}");
            Console.WriteLine($"number of negative households(s) = {Settings.negativeHouseholds}");

            Console.WriteLine("\n============================================================");
            Console.WriteLine("STANDARD RESULTS");
            Console.WriteLine("============================================================");

            Console.WriteLine($"total profit = {totalUtilityProfit}");
            Console.WriteLine($"\nprofit gain = {totalUtilityGain}");
            Console.WriteLine($"profit loss = {totalUtilityLoss}");
            Console.WriteLine($"\nprofit gain on average = {totalUtilityGain / numSeller}");
            Console.WriteLine($"profit loss on average = {totalUtilityLoss / numBuyer}");

            Console.WriteLine("\n============================================================");
            Console.WriteLine("PEER-TO-PEER MARKET RESULTS");
            Console.WriteLine("============================================================");

            Console.WriteLine($"total profit = {totalProfit}");
            Console.WriteLine($"\nprofit gain = {profitGain}");
            Console.WriteLine($"profit loss = {profitLoss}");
            Console.WriteLine($"\nprofit gain on average = {profitGain / numSeller}");
            Console.WriteLine($"profit loss on average = {profitLoss / numBuyer}");

            Console.WriteLine("\n============================================================");
            Console.WriteLine("ENERGY ALLOCATION RESULTS");
            Console.WriteLine("============================================================");

            Console.WriteLine($"\nenergy demand = {totalDemmand}");
            Console.WriteLine($"renewable energy bought = {totalRenewableBought}");
            Console.WriteLine($"utility energy bought = {totalUtilityBought}");

            Console.WriteLine($"\nenergy excess = {totalExcess}");
            Console.WriteLine($"renewable energy sold = {totalRenewableSold}");
            Console.WriteLine($"utility energy sold = {totalUtilitySold}");


            Stop();
        }
    }
}
