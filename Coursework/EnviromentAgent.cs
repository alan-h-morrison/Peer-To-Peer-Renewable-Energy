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

    private int totalProfit = 0; // total profit made by the market
    private int profitGain = 0;  // profit gained by the market
    private int averageGain = 0; // average profit gained by seller household
    private int profitLoss = 0; // profit lossed by the market
    private int averageLoss = 0; // average profit lossed by buyer households

    private int utilityGain = 0; // individual profit gained by selling all energy to utility companies
    private int utilityLoss = 0;  // individual profit gained by buying all energy to utility companies

    private int totalUtilityProfit = 0; // total profit by buying/selling energy to utility companies
    private int totalUtilityGain = 0; // total profit gained by buying all energy to utility companies
    private int totalUtilityLoss = 0; // total profit gained by buying all energy to utility companies

    private int totalDemmand = 0; // total energy demand for all households
    private int totalExcess = 0; // total energy excess for all households
    private int totalRenewableBought = 0; // total renewable energy bought
    private int totalRenewableSold = 0; // total renewable energy sold
    private int totalUtilityBought = 0; // total utility energy bought
    private int totalUtilitySold = 0; // total utility energy sold

    private int activeHouseholds = Settings.totalHouseholds; // total number of households
    private int numSeller = 0; // number of seller households
    private int numBuyer = 0; // number of buyer households
    private int numAbsent = 0; // number of absent households

    private List<Household> households; // list of households participating in the system

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

    // method to process each household's statistics and prints the system statistics to screen
    private void HandleFinish(string info)
    {
        // counter to track if all households have finished and passed statistics to environment
        activeHouseholds--;

        // using details from message, household details are added to household list
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

        // when they are no more active households
        if (activeHouseholds == 0)
        {

            Thread.Sleep(100);

            Console.WriteLine("\n******* Day Finished *******");

            foreach (var houseItem in households)
            {
                totalProfit = totalProfit + houseItem.HouseProfit;

                // process seller households' statistics
                if (houseItem.HouseType.Contains("seller"))
                {
                    numSeller++;

                    utilityGain = houseItem.EnergyDifference * houseItem.utilitySell;
                    totalUtilityGain = totalUtilityGain + utilityGain;

                    profitGain = profitGain + houseItem.HouseProfit;

                    totalExcess = totalExcess + houseItem.EnergyDifference;
                    totalRenewableSold = totalRenewableSold + houseItem.RenwableCounter;
                    totalUtilitySold = totalUtilitySold + houseItem.UtilityCounter;

                    // prints out statistics for each individual household who participated in the system
                    Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------");
                    Console.WriteLine($"[{houseItem.HouseName}] ({houseItem.HouseType}): total profit = {houseItem.HouseProfit}, potential utility profit = {utilityGain}, energy diff = {houseItem.EnergyDifference},  renewable energy = {houseItem.RenwableCounter}, utility energy = {houseItem.UtilityCounter}");
                }

                // process buyer households' statistics
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

                    // prints out statistics for each individual household who participated in the system
                    Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------");
                    Console.WriteLine($"[{houseItem.HouseName}] ({houseItem.HouseType}): total profit = {houseItem.HouseProfit}, potential utility profit = {utilityLoss}, energy diff = {houseItem.EnergyDifference},  renewable energy = {houseItem.RenwableCounter}, utility energy = {houseItem.UtilityCounter}");
                }

                // process absent households' statistics
                if (houseItem.HouseType.Contains("n/a"))
                {
                    numAbsent++;
                }
                totalUtilityProfit = totalUtilityGain + totalUtilityLoss;
            }

            averageGain = profitGain / numSeller;
            averageLoss = profitLoss / numBuyer;


            // prints out statistics for the system as a whole
            Console.WriteLine("\n============================================================");
            Console.WriteLine("HOUSEHOLDS BREAKDOWN");
            Console.WriteLine("============================================================");

            Console.WriteLine($"total households = {households.Count}");

            Console.WriteLine($"\nnumber of buyer household(s) = {numBuyer}");
            Console.WriteLine($"number of seller household(s) = {numSeller}");
            Console.WriteLine($"number of absent households(s) = {numAbsent}");

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
            Console.WriteLine($"\nprofit gain on average = {averageGain}");
            Console.WriteLine($"profit loss on average = {averageLoss}");

            Console.WriteLine("\n============================================================");
            Console.WriteLine("ENERGY ALLOCATION RESULTS");
            Console.WriteLine("============================================================");

            Console.WriteLine($"\nenergy demand = {totalDemmand}");
            Console.WriteLine($"renewable energy bought = {totalRenewableBought}");
            Console.WriteLine($"utility energy bought = {totalUtilityBought}");

            Console.WriteLine($"\nenergy excess = {totalExcess}");
            Console.WriteLine($"renewable energy sold = {totalRenewableSold}");
            Console.WriteLine($"utility energy sold = {totalUtilitySold}");

            //addRecord("D:\\University\\disposition.csv", totalProfit, profitGain, profitLoss, averageGain, averageLoss, totalDemmand, totalRenewableBought, totalUtilityBought, totalExcess, totalRenewableSold, totalUtilitySold);

            Stop();
        }
    }

    // add system statistics to csv file for experiment analysis
    public static void addRecord(string filepath, int recordProfit, int recordProfitGain, int recordProfitLoss, int recordAverageGain, int recordAverageLoss, int recordDemmand, int recordRenewBought, int recordUtilBought, int recordTotalExcess, int recordRenewSold, int recordUtilSold)
    {
        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
            {
                file.WriteLine(recordProfit.ToString() + "," + recordProfitGain.ToString() + "," + recordProfitLoss.ToString() + "," + recordAverageGain.ToString() + "," + recordAverageLoss.ToString() + "," + recordDemmand.ToString() + "," + recordRenewBought.ToString() + "," + recordUtilBought.ToString() + "," + recordTotalExcess.ToString() + "," + recordRenewSold.ToString() + "," + recordUtilSold.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
