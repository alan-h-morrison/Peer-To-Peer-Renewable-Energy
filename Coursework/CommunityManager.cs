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
        private Dictionary<string, List<string>> energyBuyers;
        private Dictionary<string, List<string>> energySellers;
        private int turnsToWait;

        private struct Bid
        {
            public string Bidder { get; set; }
            public int BidValue { get; set; }

            public Bid(string bidder, int bidValue)
            {
                Bidder = bidder;
                BidValue = bidValue;
            }
        }

        public CommunityManager()
        {
            energyBuyers = new Dictionary<string, List<string>>();
            energySellers = new Dictionary<string, List<string>>();
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
            List<string> attributes = new List<string>(details);

            attributes.RemoveAt(0);

            if(details.Contains("buy"))
            {
                energyBuyers.Add(provider, attributes);
            }
            else if(details.Contains("sell"))
            {
                energySellers.Add(provider, attributes);
            }
        }

        private void ManageEnergy()
        {
            if(energySellers.Count > 0 && energyBuyers.Count > 0)
            {
            
            }
            else if (energyBuyers.Count == 0 && energySellers.Count > 0)
            {
                foreach(var sellerItem in energySellers)
                {
                    //Console.WriteLine(sellerItem.Key);
                    string name = sellerItem.Key.ToString();
                    Send(name, "Buyer");
                }
                energySellers.Clear();
            }
            else if (energySellers.Count == 0 && energyBuyers.Count > 0)
            {
                foreach (var buyerItem in energyBuyers)
                {
                    //Console.WriteLine(buyerItem.Key);
                    string name = buyerItem.Key.ToString();
                    Send(name, "Seller");
                }
                energyBuyers.Clear();
            }
        }
    }
}