using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InveSim.Simulator
{

    public class SignalDataHolder
    {

        [JsonProperty("S")]
        public List<Signal> Signals = new List<Signal>();

        public IEnumerable<Signal> GetSignals(DateTime date) => Signals.Where(s => s.Date == date);

        public void AddSignal(DateTime date, string symbol, bool buy, bool sell) => Signals.Add(new Signal(date, symbol, buy, sell));

        public void Add(string symbol, string buyDate, string sellDate)
        {
            var buy = DateTime.ParseExact(buyDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            AddSignal(buy, symbol.Trim(), true, false);
            if (!string.IsNullOrWhiteSpace(sellDate))
            {
                var sell = DateTime.ParseExact(sellDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                AddSignal(sell, symbol.Trim(), false, true);
            }
        }

        public class Signal
        {

            [XmlAttribute]
            [JsonProperty("D")]
            public DateTime Date { get; set; }
            [XmlAttribute]
            [JsonProperty("S")]
            public string Symbol { get; set; }
            [XmlAttribute]
            [JsonIgnore]
            public bool Buy { get; set; }
            [XmlAttribute]
            [JsonIgnore]
            public bool Sell { get; set; }
            [XmlIgnore]
            [JsonProperty("T")]
            public string SerializedBuySell
            {
                get => Buy ? "B" : "S";
                set
                {
                    Buy = value == "B";
                    Sell = !Buy;
                }
            }

            public Signal() { }

            public Signal(DateTime date, string symbol, bool buy, bool sell)
            {
                Date = date;
                Symbol = symbol;
                Buy = buy;
                Sell = sell;
            }

        }

    }

}
