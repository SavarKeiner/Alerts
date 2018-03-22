using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alerts.Logic.Interfaces
{
    public interface CandleIF
    {
        long getOpenTime();
        double getOpen();
        double getClose();
        double getHigh();
        double getLow();
        double getVolume();
    }
}
