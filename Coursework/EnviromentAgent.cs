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

    private const int MinGeneration = 1; //min possible generation from renewable energy on a day for a household (in kWh)
    private const int MaxGeneration = 5; //max possible generation from renewable energy on a day for a household (in kWh)
    private const int MinDemand = 1; //min possible demand on a day for a household (in kWh)
    private const int MaxDemand = 25; //max possible demand on a day for a household (in kWh)
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
    private int averageUtilityGain = 0; // average profit gained by selling energy to utility companies
    private int averageUtilityLoss = 0; // average profit gained by buying energy to utility companies

    private int totalUtilityProfit = 0; // total profit by buying/selling energy to utility companies
    private int totalUtilityGain = 0; // total profit gained by buying all energy to utility companies
    private int totalUtilityLoss = 0; // total profit gained by buying all energy to utility companies

    private int totalDemmand = 0; // total energy demand for all households
    private int totalExcess = 0; // total energy excess for all households
    private int totalRenewableBought = 0; // total renewable energy bought
    private int totalRenewableSold = 0; // total renewable energy sold
    private int totalUtilityBought = 0; // total utility energy bought
    private int totalUtilitySold = 0; // total utility energy sold

    private double renewableDemandPercent = 0;
    private double renewableExcessPercent = 0;

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
                    Console.WriteLine($"[{houseItem.HouseName}] ({houseItem.HouseType}): total profit = {houseItem.HouseProfit}, potential utility profit = {utilityGain}, " +
                                           $"energy diff = {houseItem.EnergyDifference},  renewable energy = {houseItem.RenwableCounter}, utility energy = {houseItem.UtilityCounter}");
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
                    Console.WriteLine($"[{houseItem.HouseName}] ({houseItem.HouseType}): total profit = {houseItem.HouseProfit}, potential utility profit = {utilityLoss}, " +
                                           $"energy diff = {houseItem.EnergyDifference}, + renewable energy = {houseItem.RenwableCounter}, utility energy = {houseItem.UtilityCounter}");
                }

                // process absent households' statistics
                if (houseItem.HouseType.Contains("n/a"))
                {
                    numAbsent++;
                }
                totalUtilityProfit = totalUtilityGain + totalUtilityLoss;
            }


            // calculate the average profit gained by a seller household
            averageGain = (int)Math.Round((double)(profitGain / numSeller));

            // calculate the average profit lossed by a buyer household
            averageLoss = (int)Math.Round((double)(profitLoss / numBuyer));

            // calculate the average profit gained if energy is sold to utility company
            averageUtilityGain = (int)Math.Round((double)(totalUtilityGain / numSeller));

            // calculate the average profit gained if energy is bought to utility company
            averageUtilityLoss = (int)Math.Round((double)(totalUtilityLoss / numBuyer));

            // calculate the percentage of energy demand met through renewable energy
            renewableDemandPercent = (double)Math.Round((double)(100 * totalRenewableBought) / totalDemmand, 1);

            // calculate the percentage of energy excess sold is renewable energy
            renewableExcessPercent = (double)Math.Round((double)(100 * totalRenewableSold) / totalExcess, 1);

            //Math.Round(renewableDemandPercent, 2);
            //Math.Round(renewableExcessPercent, 2);


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

            Console.WriteLine($"total profit = {totalUtilityProfit}p");
            Console.WriteLine($"\nprofit gain = {totalUtilityGain}p");
            Console.WriteLine($"profit loss = {totalUtilityLoss}p");
            Console.WriteLine($"\nprofit gain on average = {averageUtilityGain}p");
            Console.WriteLine($"profit loss on average = {averageUtilityLoss}p");

            Console.WriteLine("\n============================================================");
            Console.WriteLine("PEER-TO-PEER MARKET RESULTS");
            Console.WriteLine("============================================================");

            Console.WriteLine($"total profit = {totalProfit}p");
            Console.WriteLine($"\nprofit gain = {profitGain}p");
            Console.WriteLine($"profit loss = {profitLoss}p");
            Console.WriteLine($"\nprofit gain on average = {averageGain}p");
            Console.WriteLine($"profit loss on average = {averageLoss}p");

            Console.WriteLine("\n============================================================");
            Console.WriteLine("ENERGY ALLOCATION RESULTS");
            Console.WriteLine("============================================================");

            Console.WriteLine($"\nenergy demand = {totalDemmand} kWh");
            Console.WriteLine($"renewable energy bought = {totalRenewableBought} kWh");
            Console.WriteLine($"utility energy bought = {totalUtilityBought} kWh");

            Console.WriteLine($"\nenergy excess = {totalExcess} kWh");
            Console.WriteLine($"renewable energy sold = {totalRenewableSold} kWh");
            Console.WriteLine($"utility energy sold = {totalUtilitySold} kWh");

            Console.WriteLine($"\npercentage of renewable energy bought by households: {renewableDemandPercent}%");
            Console.WriteLine($"percentage of renewable energy sold by households: {renewableExcessPercent}%");

            // adds statisitics to .csv file for analysis
            addRecord("D:\\University\\stats.csv", averageGain, averageLoss, renewableDemandPercent, renewableExcessPercent, numBuyer, numSeller, numAbsent);

            Stop();
        }
    }

    // add system statistics to csv file for experiment analysis
    public static void addRecord(string filepath, 
                                     int recordAverageGain,
                                     int recordAverageLoss, 
                                     double recordRenewDemandPercent, 
                                     double recordRenewExcessPercent, 
                                     int recordBuyers,
                                     int recordSellers,
                                     int recordAbsent)
    {
        try
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
            {
                file.WriteLine(recordAverageGain.ToString() + ","
                                   + recordAverageLoss.ToString() + ","
                                   + recordRenewDemandPercent.ToString() + ","
                                   + recordRenewExcessPercent.ToString() + ","
                                   + recordBuyers.ToString() + ","
                                   + recordSellers.ToString() + ","
                                   + recordAbsent.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
