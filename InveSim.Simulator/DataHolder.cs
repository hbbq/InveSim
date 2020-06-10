using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace InveSim.Simulator
{

    public class DataHolder
    {

        [JsonProperty("DH")]
        public DateHandler DateHandler = new DateHandler();
        [JsonProperty("SY")]
        public List<SymbolDataHolder> Symbols = new List<SymbolDataHolder>();
        [JsonProperty("SG")]
        public SignalDataHolder Signals = new SignalDataHolder();

        public SymbolDataHolder GetSymbol(string symbol)
        {
            var sym = Symbols.FirstOrDefault(s => s.Symbol == symbol);
            if (sym != null) return sym;
            sym = new SymbolDataHolder() { Symbol = symbol };
            Symbols.Add(sym);
            return sym;
        }

        public string Serialize()
        {
            var serializer = new XmlSerializer(typeof(DataHolder));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, this);
                return writer.ToString();
            }
        }

        public static DataHolder Deserialize(string serialized)
        {
            if (string.IsNullOrWhiteSpace(serialized)) return new DataHolder();
            var serializer = new XmlSerializer(typeof(DataHolder));
            using (var reader = new StringReader(serialized))
            {
                return (DataHolder)serializer.Deserialize(reader);
            }
        }

        public string JsonSerialize()
        {
            var serializer = new JsonSerializer
            {
                //serializer.Formatting = Formatting.Indented;
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyMMdd"
            };

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, this);
                return writer.ToString();
            }
        }

        public static DataHolder JsonDeserialize(string serialized)
        {
            if (string.IsNullOrWhiteSpace(serialized)) return new DataHolder();
            var serializer = new JsonSerializer
            {
                //serializer.Formatting = Formatting.Indented;
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyMMdd"
            };
            using (var tr = new StringReader(serialized))
            using (var reader = new JsonTextReader(tr))
            {
                return (DataHolder)serializer.Deserialize<DataHolder>(reader);
            }
        }

    }

}
