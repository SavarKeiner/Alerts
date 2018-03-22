using Alerts.Logic.Enums;
using Alerts.Logic.Events;
using Alerts.Logic.Interfaces;
using Alerts.Logic.RESTObjects;
using Alerts.UI;
using Newtonsoft.Json;
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
        public Exchanges Exchange { get; } = Exchanges.Binance;
        public Coins Coin { get; set; }
        public Coins Pair { get; set; }
        public string ApiUrl { get; } = "https://api.binance.com/api/v1";
        public int maxLimit { get; } = 200;
        public int minLimit { get; } = 2;

        public EventHandler<CandlePullEventArgs> candlePulled;
        private Dictionary<CandleWidth, CancellationTokenSource> stopAsyncToken = new Dictionary<CandleWidth, CancellationTokenSource>();
        private Dictionary<CandleWidth, List<CandleBinance>> dictCandle = new Dictionary<CandleWidth, List<CandleBinance>>();

        private AlertLayout parent;

        public Binance(AlertLayout parent, Coins Coin, Coins Pair)
        {
            this.parent = parent;
            this.Coin = Coin;
            this.Pair = Pair;

            foreach (CandleWidth k in Enum.GetValues(typeof(CandleWidth)))
            {
                stopAsyncToken.Add(k, null);
                dictCandle.Add(k, new List<CandleBinance>());
            }


        }

        public async void CandlePull(CandleWidth width, CancellationToken token)
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
                        if (clist.Count == maxLimit)
                        {
                            limit = minLimit;
                            if (_width != CandleWidth.INIT)
                            {
                                request = new RestRequest("/klines?symbol=" + Coin + Pair + "&interval=" + App.candleStickWidthToString(width) + "&limit=" + limit);
                            }
                        }

                        IRestResponse response = client.Execute(request);

                        List<CandleBinance> list = JsonConvert.DeserializeObject<List<CandleBinance>>(response.Content);

                        if (list.Count == maxLimit || _width == CandleWidth.INIT)
                        {
                            clist = list;
                        }
                        else if (list.Count < maxLimit && clist.Count == maxLimit)
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
                        OnCandlePull(args);

                        Thread.Sleep(1000);
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

        public void add(AlertCard card, List<AlertCard> childList)
        {
            if (childList.Count == 0)
            {
                CancellationTokenSource source = new CancellationTokenSource();

                stopAsyncToken[CandleWidth.INIT] = source;
                parent.Header.Symbol = card.Coin + card.Pair.ToString();
                parent.Header.Exchange = card.Exchange;
                CandlePull(CandleWidth.INIT, source.Token);
                candlePulled += parent.Header.initPull;
            }

            if (childList.Find(x => x.KlineWidth == card.KlineWidth) == null)
            {
                CancellationTokenSource source;
                stopAsyncToken.TryGetValue(card.KlineWidth, out source);
                if (source == null)
                {
                    source = new CancellationTokenSource();

                    stopAsyncToken[card.KlineWidth] = source;

                    CandlePull(card.KlineWidth, source.Token);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DBG-ERROR 1: " + card.KlineWidth);
                }
            }
            else
            {

            }

            candlePulled += card.CandlePull;
            childList.Add(card);
            childList.Sort((x, y) => x.KlineWidth.CompareTo(y.KlineWidth));

        }

        public void remove(AlertCard card, List<AlertCard> childList)
        {
            candlePulled -= card.CandlePull;
            childList.Remove(card);

            if (childList.Count == 0)
            {
                candlePulled -= parent.Header.initPull;
                CancellationTokenSource source;
                stopAsyncToken.TryGetValue(CandleWidth.INIT, out source);
                if (source != null)
                {
                    source.Cancel();
                    source.Dispose();
                }
            }

            if (childList.Find(x => x.KlineWidth == card.KlineWidth) == null)
            {
                CancellationTokenSource source;
                stopAsyncToken.TryGetValue(card.KlineWidth, out source);
                if (source != null)
                {
                    List<CandleBinance> clist;
                    dictCandle.TryGetValue(card.KlineWidth, out clist);
                    clist?.Clear();

                    source.Cancel();
                    source.Dispose();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DBG-ERROR 2: " + card.KlineWidth);
                }
            }
            childList.Sort((x, y) => x.KlineWidth.CompareTo(y.KlineWidth));
        }

        protected virtual void OnCandlePull(CandlePullEventArgs e)
        {
            candlePulled?.Invoke(this, e);
        }
    }
}
