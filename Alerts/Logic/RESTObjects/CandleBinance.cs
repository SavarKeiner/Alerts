using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alerts.Logic.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Alerts.Logic.RESTObjects
{

    [JsonConverter(typeof(CandleBinanceConverter))]
    public class CandleBinance : CandleIF
    {
        public long OpenTime { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public long CloseTime { get; set; }
        public double QuoteAssetVolume { get; set; }
        public long NumberOfTrades { get; set; }
        public double TakerBuyBaseAssetVolume { get; set; }
        public double TakerBuyQuoteAssetVolume { get; set; }
        public double Ignore { get; set; }

        public double getClose()
        {
            return Close;
        }

        public double getHigh()
        {
            return High;
        }

        public double getLow()
        {
            return Low;
        }

        public double getOpen()
        {
            return Open;
        }

        public long getOpenTime()
        {
            return OpenTime;
        }

        public double getVolume()
        {
            return Volume;
        }
    }

    public class CandleBinanceConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.Name.Equals("CandleBinance");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JArray array = JArray.Load(reader);
            return new CandleBinance
            {
                OpenTime = array[0].ToObject<long>(),
                Open = array[1].ToObject<double>(),
                High = array[2].ToObject<double>(),
                Low = array[3].ToObject<double>(),
                Close = array[4].ToObject<double>(),
                Volume = array[5].ToObject<double>(),
                CloseTime = array[6].ToObject<long>(),
                QuoteAssetVolume = array[7].ToObject<double>(),
                NumberOfTrades = array[8].ToObject<long>(),
                TakerBuyBaseAssetVolume = array[9].ToObject<double>(),
                TakerBuyQuoteAssetVolume = array[10].ToObject<double>(),
                Ignore = array[11].ToObject<double>()
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var candle = value as CandleBinance;
            JArray array = new JArray();
            array.Add(candle.OpenTime);
            array.Add(candle.Open);
            array.Add(candle.High);
            array.Add(candle.Low);
            array.Add(candle.Close);
            array.Add(candle.Volume);
            array.Add(candle.QuoteAssetVolume);
            array.Add(candle.NumberOfTrades);
            array.Add(candle.NumberOfTrades);
            array.Add(candle.TakerBuyBaseAssetVolume);
            array.Add(candle.TakerBuyQuoteAssetVolume);
            array.Add(candle.Ignore);
            array.WriteTo(writer);
        }
    }
}
