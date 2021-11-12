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

        private struct Bid
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
            var bid = new Bid(provider, Convert.ToInt32(details[2]), Convert.ToInt32(details[1]));

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
            if(buyerBids.Count > 0 && sellerBids.Count > 0)
            {
                // sort seller bidding list by asceding value
                sellerBids.Sort((s1, s2) => s1.BidValue.CompareTo(s2.BidValue));

                // sort buyer bidding list by asceding value and the reversing the list to sort by descending value
                buyerBids.Sort((s1, s2) => s1.BidValue.CompareTo(s2.BidValue));
                buyerBids.Reverse();

            }
            else if (buyerBids.Count == 0 && sellerBids.Count > 0)
            {
                foreach(var sellerItem in sellerBids)
                {
                    Send(sellerItem.Bidder, "no-buyers");
                }
                sellerBids.Clear();
                Stop();
            }
            else if (sellerBids.Count == 0 && buyerBids.Count > 0)
            {
                foreach (var buyerItem in buyerBids)
                {
                    Send(buyerItem.Bidder, "no-sellers");
                }
                buyerBids.Clear();
                Stop();
            }
        }
    }
}