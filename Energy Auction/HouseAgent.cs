using ActressMas;

namespace EnergyAuction;

public class HouseAgent : Agent
{
    private int _demand;
    private int _generation;
    private int _priceToBuy;
    private int _priceToSell;
    private int _valuation;
    private Boolean _wantToBuy;
    private Boolean _wantToSell;
    private int _currentPrice;
    private int _currentBid;
    public override void Setup()
    {
        Send("environment","start");
    }

    public override void Act(Message message)
    {
        Random random = new Random();
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out string parameters);
                switch (action)
                {
                    case "inform":
                        _demand = Int32.Parse(parameters.Split(" ")[0]);
                        _generation = Int32.Parse(parameters.Split(" ")[1]);
                        _priceToBuy = Int32.Parse(parameters.Split(" ")[2]);
                        _priceToSell = Int32.Parse(parameters.Split(" ")[3]);
                        _valuation = random.Next(_priceToSell, _priceToBuy);
                        HandleBuyOrSell(_demand, _generation, _valuation);
                        break;

                    case "price":
                        if (_wantToBuy)
                        {
                            _currentPrice = Convert.ToInt32(parameters);
                            HandleBid();
                        }
                        break;
                    
                    case "delay":
                        HandleBuyOrSell(_demand, _generation, _valuation);
                        break;
                    
                    case "gain":
                        _generation += 1;
                        break;

                    case "reduce":
                        _generation -= 1;
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

    private void HandleBuyOrSell(int demand, int generation, int valuation)
    {
        if (demand < generation)
        {
            _wantToSell = true;
            Send("auctioneer", $"sell {valuation}");
        }

        else if (demand > generation)
        {
            _wantToBuy = true;
            Send("auctioneer", "buy");
        }

        else
        {
            Console.WriteLine();
            Stop();
        }
    }
    
    private void HandleBid()
    {
        if (_currentPrice < _valuation)
        {
            _currentBid = _currentPrice;
            Send("auctioneer",$"bid {_currentBid}");
        }
        else
        {
            Send("auctioneer",$"wait ");
        }
    }
}