/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: English auction with broadcast using the ActressMas      *
 *               framework                                                *
 *  Copyright:   (c) 2018, Florin Leon                                    *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/

using ActressMas;
using System;
using Microsoft.VisualBasic.CompilerServices;

namespace EnergyAuction
{
    public class AuctioneerAgent : Agent
    {
        private struct Sell
        {
            public string Seller { get; set; }
            public int SaleValue { get; set; }

            public Sell(string seller, int saleValue)
            {
                Seller = seller;
                SaleValue = saleValue;
            }
        }
        
        private struct Buy
        {
            public string Buyer { get; set; }
            public int BuyValue { get; set; }

            public Buy(string buyer, int buyValue)
            {
                Buyer = buyer;
                BuyValue = buyValue;
            }
        }

        private List<Sell> sellers;
        private List<Buy> buyers;
        private Buy buyer = new Buy();
        private Sell seller = new Sell();
        private int decreases = 0;
        private int _turnsToWait;
        private int bidlimit = 0;
        private int _startingPrice;
        private int _noSold = 0;

        public AuctioneerAgent()
        {
            sellers = new List<Sell>();
            buyers = new List<Buy>();
        }

        public override void Setup()
        {
            buyer.Buyer = "";
            seller.Seller = "";
            _turnsToWait = 2;
        }

        public override void Act(Message message)
        {
            if (sellers.Count + 1 > 0) 
                try
                {
                    Console.WriteLine($"\t{message.Format()}");
                    message.Parse(out string action, out string parameters);
                    switch (action)
                    {
                        case "sell":
                            HandleSeller(message.Sender, Convert.ToInt32(parameters));
                            break;
                    
                        case "buy":
                            HandleBuyer(message.Sender);
                            break;
                        
                        case "bid":
                            bidlimit++;
                            HandleBid(message.Sender, Convert.ToInt32(parameters));
                            bidlimit = 0;
                            break;
                        
                        case "wait":
                            decreases++;
                            HandleDecrease(message.Sender);
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
        

        private void HandleDecrease(string sender)
        {
            if (decreases == buyers.Count)
            {
                _startingPrice -= Settings.Increment;
                foreach (var variableBuyer in buyers)
                {
                    Send(variableBuyer.Buyer, $"price {_startingPrice}");
                }

                decreases = 0;
            }
        }

        private void HandleBid(string sender, int price)
        {
            buyer.BuyValue = price;
            buyer.Buyer = sender;
            HandleFinish();
        }

        private void HandleSeller(string sender, int price)
        {
            sellers.Add(new Sell(sender, price));
        }
        
        private void HandleBuyer(string sender)
        {
            if (sellers.Count > 0)
            {
                _startingPrice = sellers[0].SaleValue;
                buyers.Add(new Buy(sender, 0));
                Send(sender, $"price {_startingPrice}");
            }

            else
            {
                Console.WriteLine("No sellers, delaying...");
                Send(sender, "delay");
            }
        }

        

        private void HandleFinish()
        {
            if ( buyer.Buyer == "") // no bids above reserve price
            {
                Console.WriteLine("[auctioneer]: Auction finished. No winner.");
                Broadcast("winner none");
            }
            else
            {
                Console.WriteLine($"[auctioneer]: {sellers[0].Seller} auction finished. Sold to {buyer.Buyer} for price {buyer.BuyValue}.");
                Send(sellers[0].Seller, "reduce");
                Send(buyer.Buyer, "gain");
                sellers.Clear();
                buyers.Clear();
                Broadcast("delay");
            }
        }
    }
}