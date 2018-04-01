using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alerts.Logic.Enums;
using Alerts.Logic.Events;
using Alerts.UI;

namespace Alerts.Logic.Interfaces
{
    public interface ExchangeIF
    {
        Exchanges Exchange { get; }
        string ApiUrl { get; }

        void CandlePull(CandleWidth width, Coins Coin, Coins Pair, CancellationToken token);

        void remove(AlertCard card, List<AlertCard> childList);
        void add(AlertCard card, Coins Coin, Coins Pair, List<AlertCard> childList);
        List<ImageTextItem> GetPairList();
        List<ImageTextItem> GetCoinsListFromPair(Coins Pair);
    }
}
