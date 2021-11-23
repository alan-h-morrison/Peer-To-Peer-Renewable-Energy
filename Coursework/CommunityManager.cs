﻿using System;
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

        private class Bid
        {
            public string Bidder { get; set; }
            public int BidValue { get; set; }
            public int EnergyDifference { get; set; }

            public Bid(string bidder, int bidValue, int difference)
            {
                Bidder = bidder;
                BidValue = bidValue;
                EnergyDifference = difference;
            }

            public void DecreaseEnergy()
            {
                this.EnergyDifference = EnergyDifference - 1;
            }
        }

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

        public override void ActDefault()
        {
            if (--turnsToWait <= 0)
                ManageEnergy();
        }


        private void HandleRegister(string provider, List<string> details)
        {
            var bid = new Bid(provider, Convert.ToInt32(details[1]), Convert.ToInt32(details[2]));

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
                sellerBids.Sort((s1, s2) => s1.BidValue.CompareTo(s2.BidValue));
                //sellerBids.Reverse();

                // sort buyer bidding list by asceding value and the reversing the list to sort by descending value
                buyerBids.Sort((s1, s2) => s1.BidValue.CompareTo(s2.BidValue));
                buyerBids.Reverse();

                if ((buyerBids.Count > 0 && sellerBids.Count > 0))
                {
                    var transaction = false;

                    for (int i = buyerBids.Count - 1; i >= 0; i--)
                    {
                        for (int j = sellerBids.Count - 1; j >= 0; j--)
                        {
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

                            if (buyerBids[i].BidValue > sellerBids[j].BidValue)
                            {
                                buyerBids[i].DecreaseEnergy();
                                sellerBids[j].DecreaseEnergy();

                                int equilibriumPrice = (buyerBids[i].BidValue + sellerBids[j].BidValue) / 2;
                                Settings.Increment();
                                Send(buyerBids[i].Bidder, $"bought {equilibriumPrice}");

                                Settings.Increment();
                                Send(sellerBids[j].Bidder, $"sold {equilibriumPrice}");

                                transaction = true;
                            }
                        }
                    }

                    if (!transaction)
                    {
                        Console.WriteLine("No more transactions can take place");

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