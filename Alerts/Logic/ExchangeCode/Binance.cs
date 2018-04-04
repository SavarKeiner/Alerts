using Alerts.Logic.Enums;
using Alerts.Logic.Events;
using Alerts.Logic.Interfaces;
using Alerts.Logic.RESTObjects;
using Alerts.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alerts.Logic.ExchangeCode
{
    public class Binance : ExchangeIF
    {
        private static Binance instance;
        public static Binance Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Binance();
                }
                return instance;
            }
        }

        public Exchanges Exchange { get; } = Exchanges.Binance;
        //public Coins Coin { get; set; }
        //public Coins Pair { get; set; }
        public string ApiUrl { get; } = "https://api.binance.com/api/v1";
        public int maxLimit { get; } = 200;
        public int minLimit { get; } = 2;

        public EventHandler<CandlePullEventArgs> candlePulled;
        private Dictionary<CandleWidth, CancellationTokenSource> stopAsyncToken = new Dictionary<CandleWidth, CancellationTokenSource>();
        private Dictionary<CandleWidth, List<CandleBinance>> dictCandle = new Dictionary<CandleWidth, List<CandleBinance>>();

        private Dictionary<Coins, List<Coins>> paringCoinDict = new Dictionary<Coins, List<Coins>>();
        private Dictionary<ImageTextItem, List<ImageTextItem>> uiCoinsList = new Dictionary<ImageTextItem, List<ImageTextItem>>();

        public int sleepTime = 1000;

        private Binance()
        {
            setPairDict();

            foreach (CandleWidth k in Enum.GetValues(typeof(CandleWidth)))
            {
                stopAsyncToken.Add(k, null);
                dictCandle.Add(k, new List<CandleBinance>());
            }
        }

        public async void CandlePull(CandleWidth width, Coins Coin, Coins Pair, CancellationToken token)
        {
            await Task.Run(() =>
            {
                try
                {
                    RestClient client = new RestClient(ApiUrl);
                    RestRequest request;
                    CandleWidth _width = width;
                    List<CandleBinance> clist;
                    int limit = maxLimit;
                    dictCandle.TryGetValue(_width, out clist);
                    bool newList = false;

                    if (width == CandleWidth.INIT)
                    {
                        width = CandleWidth.m5;
                        request = new RestRequest("/klines?symbol=" + Coin + Pair + "&interval=" + App.candleStickWidthToString(width) + "&limit=" + minLimit);
                    }
                    else
                    {
                        request = new RestRequest("/klines?symbol=" + Coin + Pair + "&interval=" + App.candleStickWidthToString(width) + "&limit=" + maxLimit);
                    }

                    while (!token.IsCancellationRequested)
                    {
                        if (clist.Count <= maxLimit && clist.Count > minLimit)
                        {
                            limit = minLimit;
                            if (_width != CandleWidth.INIT)
                            {
                                request = new RestRequest("/klines?symbol=" + Coin + Pair + "&interval=" + App.candleStickWidthToString(width) + "&limit=" + limit);
                            }
                        } else if(clist.Count == 0)
                        {
                            request = new RestRequest("/klines?symbol=" + Coin + Pair + "&interval=" + App.candleStickWidthToString(width) + "&limit=" + limit);
                        }

                        IRestResponse response = client.Execute(request);

                        System.Diagnostics.Debug.WriteLine("Response Candle: " + response.ErrorMessage + " " + response.StatusCode + " " + response.IsSuccessful + " " + response.StatusDescription + " " + response.ErrorMessage + " " +response.ResponseStatus);

                        if((int)response.StatusCode == 429)
                        {
                            sleepTime = 3000;
                        }

                        if (response.IsSuccessful == false)
                        {
                            if(clist.Count > 0)
                            {
                                clist.Clear();
                                limit = maxLimit;
                                newList = true;
                            }
                            Thread.Sleep(5000);
                            continue;
                        }


                        List<CandleBinance> list = JsonConvert.DeserializeObject<List<CandleBinance>>(response.Content);
                        if (_width == CandleWidth.INIT || (list.Count > minLimit && list.Count <= maxLimit))
                        {
                            clist = list;
                        }
                        else if (list.Count == minLimit && clist.Count <= maxLimit)
                        {
                            CandleBinance clast = clist[clist.Count - 1];
                            CandleBinance last = list[list.Count - 1];

                            if (last.OpenTime == clast.OpenTime)
                            {
                                clist[clist.Count - 1] = last;
                                ;
                                clast = last;
                            }
                            else if (last.OpenTime > clast.OpenTime)
                            {
                                clist.RemoveAt(0);
                                clist.Add(last);
                            }

                            /*System.Diagnostics.Debug.WriteLine("Pulled: " + clast.OpenTime + " " + last.OpenTime);

                            foreach(CandleBinance cb in clist)
                            {
                                System.Diagnostics.Debug.WriteLine(cb.OpenTime);
                            }
                            System.Diagnostics.Debug.WriteLine("---------------");*/
                        }

                        CandlePullEventArgs args = new CandlePullEventArgs();
                        args.Width = _width;
                        args.candleList = clist.ToList<CandleIF>();
                        args.newList = newList;
                        OnCandlePull(args);


                        Thread.Sleep(sleepTime);
                    }
                } catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message + " " + e.StackTrace);
                    throw;
                }

            }, token);
        }

        private void addToCList(List<CandleBinance> clist, CandleBinance candle)
        {
            CandleBinance last = clist[clist.Count - 1];

            if(candle.OpenTime >= last.OpenTime)
            {
                clist[clist.Count - 1] = candle;
            }
        }

        public void add(AlertCard card, Coins Coin, Coins Pair, List<AlertCard> childList)
        {
            if (childList.Count == 0)
            {
                CancellationTokenSource source = new CancellationTokenSource();

                stopAsyncToken[CandleWidth.INIT] = source;
                CandlePull(CandleWidth.INIT, Coin, Pair, source.Token);
            }

            if (childList.Find(x => x.CandleWidth == card.CandleWidth) == null)
            {
                CancellationTokenSource source;
                stopAsyncToken.TryGetValue(card.CandleWidth, out source);
                if (source == null || source.IsCancellationRequested)
                {
                    source = new CancellationTokenSource();

                    stopAsyncToken[card.CandleWidth] = source;

                    CandlePull(card.CandleWidth, Coin, Pair, source.Token);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DBG-ERROR 1: " + card.CandleWidth);
                }
            }
            else
            {

            }

            candlePulled += card.CandlePull;
            childList.Add(card);
            childList.Sort((x, y) => x.CandleWidth.CompareTo(y.CandleWidth));

        }

        public void remove(AlertCard card, List<AlertCard> childList)
        {
            System.Diagnostics.Debug.WriteLine("DBG_REMOVE");

            candlePulled -= card.CandlePull;

            childList.Remove(card);

            if (childList.Count == 0)
            {
                CancellationTokenSource source;
                stopAsyncToken.TryGetValue(CandleWidth.INIT, out source);
                if (source != null)
                {
                    source.Cancel();
                    source.Dispose();
                }
            }

            if (childList.Find(x => x.CandleWidth == card.CandleWidth) == null)
            {
                CancellationTokenSource source;
                stopAsyncToken.TryGetValue(card.CandleWidth, out source);
                if (source != null)
                {
                    List<CandleBinance> clist;
                    dictCandle.TryGetValue(card.CandleWidth, out clist);
                    clist?.Clear();

                    source?.Cancel();
                    source?.Dispose();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DBG-ERROR 2: " + card.CandleWidth);
                }
            }
            childList.Sort((x, y) => x.CandleWidth.CompareTo(y.CandleWidth));
        }

        public List<ImageTextItem> GetPairList()
        {
            return new List<ImageTextItem>(uiCoinsList.Keys).OrderBy( x => x.coin.ToString()).ToList();
        }

        public List<ImageTextItem> GetCoinsListFromPair(Coins Pair)
        {
            return uiCoinsList.First(x => x.Key.coin == Pair).Value.OrderBy(x => x.coin.ToString()).ToList();
        }

        private async void setPairDict()
        {
            await Task.Run(() =>
            {
                RestClient client = new RestClient(ApiUrl);
                RestRequest request = new RestRequest("/exchangeInfo");
                IRestResponse response = client.Execute(request);

                System.Diagnostics.Debug.WriteLine("Response: " + response.ErrorMessage + " " + response.StatusCode + " " + response.IsSuccessful);

                try
                {
                    JToken jt = JToken.Parse(response.Content).SelectToken("symbols");

                    foreach (JToken token in jt)
                    {
                        Coins k; //pair
                        Coins v = Coins.INIT; //coin

                        if (Enum.TryParse<Coins>(token.SelectToken("quoteAsset").ToString(), out k) == true && Enum.TryParse<Coins>(token.SelectToken("baseAsset").ToString(), out v) == true)
                        {
                            if ("456".Equals(token.SelectToken("quoteAsset").ToString()) == false)
                            {
                                if (!paringCoinDict.ContainsKey(k)) //pair does not exist
                                {
                                    List<Coins> lv = new List<Coins>();
                                    lv.Add(v);

                                    paringCoinDict.Add(k, lv);
                                }
                                else //pair exists
                                {
                                    List<Coins> lv;
                                    paringCoinDict.TryGetValue(k, out lv);
                                    lv.Add(v);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("DBG-EXCEPTION!!!!: " + e.Message + " " + e.StackTrace);
                }
            });

            foreach (KeyValuePair<Coins, List<Coins>> entry in paringCoinDict) //every pair
            {
                ImageTextItem item = new ImageTextItem();

                try
                {
                    item.image.Source = new Uri("/UI/Icons/CoinIcons/" + entry.Key + ".svg", UriKind.Relative);
                    item.text.Content = entry.Key;
                    item.coin = entry.Key;
                    uiCoinsList.Add(item, new List<ImageTextItem>());
                }
                catch (Exception)
                {
                    item.image.Source = new Uri("/UI/Icons/CoinIcons/WhiteCircle.svg", UriKind.Relative);
                    item.text.Content = entry.Key;
                    item.coin = entry.Key;
                    uiCoinsList.Add(item, new List<ImageTextItem>());
                }


                foreach (Coins c in entry.Value) //every coin to this pair
                {
                    try
                    {
                        ImageTextItem entryCoin = new ImageTextItem();
                        entryCoin.image.Source = new Uri("/UI/Icons/CoinIcons/" + c + ".svg", UriKind.Relative);
                        entryCoin.text.Content = c;
                        entryCoin.coin = c;

                        List<ImageTextItem> list;
                        uiCoinsList.TryGetValue(item, out list);
                        list.Add(entryCoin);
                    }
                    catch (Exception)
                    {
                        ImageTextItem entryCoin = new ImageTextItem();
                        entryCoin.image.Source = new Uri("/UI/Icons/CoinIcons/WhiteCircle.svg", UriKind.Relative);
                        entryCoin.text.Content = c;
                        entryCoin.coin = c;

                        List<ImageTextItem> list;
                        uiCoinsList.TryGetValue(item, out list);
                        list.Add(entryCoin);
                    }
                }
            }
        }

        protected virtual void OnCandlePull(CandlePullEventArgs e)
        {
            candlePulled?.Invoke(this, e);
        }
    }
}
