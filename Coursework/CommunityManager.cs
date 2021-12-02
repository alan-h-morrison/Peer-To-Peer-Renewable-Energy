using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActressMas;

namespace Coursework
{
    class CommunityManager : Agent
    {
        private int turnsToWait;
        private List<Bid> sellerBids;
        private List<Bid> buyerBids;

        public CommunityManager()
        {
            sellerBids = new List<Bid>();
            buyerBids = new List<Bid>();

            turnsToWait = 2;
        }

        public override void Setup()
        {
            Console.WriteLine($"Starting [{Name}]");
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out List<string> parameters);

                switch (action)
                {
                    case "register":
                        HandleRegister(message.Sender, parameters);
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

        // After a certain about of idle turns where the community manager hasn't been messaged
        // the community manager starts managing all the bids it has received
        public override void ActDefault()
        {
            if (--turnsToWait <= 0)
                ManageEnergy();
        }

        // Method used to register buyers/sellers into the market
        private void HandleRegister(string provider, List<string> details)
        {
            // new bid that contains which household its from, their buying/selling price and their energy difference
            var bid = new Bid(provider, Convert.ToInt32(details[1]), Convert.ToInt32(details[2]));

            // determines if a bid should be added to buyer or seller bid list
            if (details.Contains("buy")) 
            {
                buyerBids.Add(bid);
            }
            else if(details.Contains("sell"))
            {
                sellerBids.Add(bid);
            }
        }

        private void ManageEnergy()
        {
            try
            {
                // sort seller bidding list by asceding value and the reversing the list to sort by descending value
                sellerBids.Sort((s1, s2) => s1.BidValue.CompareTo(s2.BidValue));

                // sort buyer bidding list by descending value and the reversing the list to sort by descending value
                buyerBids.Sort((s1, s2) => s1.BidValue.CompareTo(s2.BidValue));
                buyerBids.Reverse();

                // there must both buyer bid and seller bid in each bidding list to allow the community manager to pair bids
                if ((buyerBids.Count > 0 && sellerBids.Count > 0))
                {
                    // bool variable used to track if a takes place
                    var transaction = false;

                    // a nested for loop used to compare all buyer bids against all seller bids
                    for (int i = buyerBids.Count - 1; i >= 0; i--)
                    {
                        for (int j = sellerBids.Count - 1; j >= 0; j--)
                        {
                            // if a bid has no energy difference its demand has been met and can be removed from the bidding list it belongs to
                            if (buyerBids[i].EnergyDifference == 0)
                            {
                                buyerBids.RemoveAt(i);
                                break;
                            }

                            if (sellerBids[j].EnergyDifference == 0)
                            {
                                sellerBids.RemoveAt(j);
                                break;
                            }

                            // if a buyer bid's value is higher than a seller bid's value then the two bids can be matched to eachother
                            if (buyerBids[i].BidValue > sellerBids[j].BidValue)
                            {
                                // energy difference for both bids are decreased
                                buyerBids[i].DecreaseEnergy();
                                sellerBids[j].DecreaseEnergy();

                                // an equilibrium price between the buyer and seller is found
                                int equilibriumPrice = (buyerBids[i].BidValue + sellerBids[j].BidValue) / 2;

                                // Global message counter is incremeneted when a message is sent
                                Settings.Increment();
                                // Message send to buyer informing they have bought an energy lot and what price they bought it at
                                Send(buyerBids[i].Bidder, $"bought {equilibriumPrice}");

                                // Global message counter is incremeneted when a message is sent
                                Settings.Increment();
                                // Message send to seller informing they have bought an energy lot and what price they bought it at
                                Send(sellerBids[j].Bidder, $"sold {equilibriumPrice}");

                                // a transaction has taken place therefore the bool value is set to true
                                transaction = true;
                            }
                        }
                    }

                    // when no transaction takes place, no more buyer bids can be matched to seller bids therefore the market ends
                    // both buyers and sellers are sent a message informing them to buy/sell the rest of their energy from utility companies
                    if (!transaction)
                    {
                        Console.WriteLine($"[{Name}]: No more transactions can take place");

                        foreach (var buyerItem in buyerBids)
                        {
                            Settings.Increment();
                            Send(buyerItem.Bidder, "no-sellers");
                        }

                        foreach (var sellerItem in sellerBids)
                        {
                            Settings.Increment();
                            Send(sellerItem.Bidder, "no-buyers");
                        }

                        Stop();
                    }
                }
                else if (buyerBids.Count == 0 && sellerBids.Count > 0)
                {
                    // the sellers left in the seller bids list are sent "no-buyers" to inform them to sell the rest of their renewable energy to utility companies
                    foreach (var sellerItem in sellerBids)
                    {
                        Settings.Increment();
                        Send(sellerItem.Bidder, "no-buyers");
                    }
                    sellerBids.Clear();
                    Stop();
                }
                else if (sellerBids.Count == 0 && buyerBids.Count > 0)
                {
                    foreach (var buyerItem in buyerBids)
                    {
                        // the buyers left in the buyer bids list are sent "no-seleers" to inform them to buy the rest of their renewable energy to utility companies
                        Settings.Increment();
                        Send(buyerItem.Bidder, "no-sellers");
                    }
                    buyerBids.Clear();
                    Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}