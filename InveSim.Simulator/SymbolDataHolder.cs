using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InveSim.Simulator
{

    public class SymbolDataHolder
    {

        [XmlAttribute("S")]
        [JsonProperty("S")]
        public string Symbol { get; set; }
        [JsonProperty("P")]
        [JsonIgnore]
        public List<DataPoint> DataPoints = new List<DataPoint>();

        [JsonProperty("X")]
        public string Serialized
        {
            get => string.Join("#", DataPoints.Select(p => p.Serialized).ToArray());
            set
            {
                DataPoints = new List<DataPoint>();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    foreach(var p in value.Split('#'))
                    {
                        DataPoints.Add(new DataPoint() { Serialized = p });
                    }
                }
            }
        }

        public void SetOpen(DateTime date, decimal value)
        {
            var point = DataPoints.FirstOrDefault(p => p.Date == date);
            if (point == null)
            {
                point = new DataPoint(date);
                DataPoints.Add(point);
            }
            point.Open = value;
        }

        public void SetClose(DateTime date, decimal value)
        {
            var point = DataPoints.FirstOrDefault(p => p.Date == date);
            if (point == null)
            {
                point = new DataPoint(date);
                DataPoints.Add(point);
            }
            point.Close = value;
        }

        public decimal? GetOpen(DateTime date) => DataPoints.FirstOrDefault(p => p.Date == date)?.Open;
        public decimal? GetClose(DateTime date) => DataPoints.FirstOrDefault(p => p.Date == date)?.Close;

        public class DataPoint
        {

            [XmlAttribute("D")]
            [JsonIgnore]
            public DateTime Date { get; set; }
            [XmlElement("O")]
            [JsonIgnore]
            public decimal? Open { get; set; }
            [XmlElement("C")]
            [JsonIgnore]
            public decimal? Close { get; set; }

            [JsonProperty("X")]
            public string Serialized
            {
                get => $"{Date:yyyyMMdd}|{Open}|{Close}";
                set
                {
                    var p = value.Split('|');
                    Date = DateTime.ParseExact(p[0], "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    Open = p[1] == "" ? null : (decimal?)decimal.Parse(p[1]);
                    Close = p[2] == "" ? null : (decimal?)decimal.Parse(p[2]);
                }
            }

            public DataPoint() { }

            public DataPoint(DateTime date, decimal? open = null, decimal? close = null)
            {
                Date = date;
                Open = open;
                Close = close;
            }

        }

    }

}
