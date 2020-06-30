using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using YahooFinanceAPI;
using YahooFinanceAPI.Models;

namespace InveSim.App
{

    class Program
    {

        static void Main(string[] args)
        {

            var infoPath = @"Dropbox\info.json";

            var jsonPath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), infoPath);

            if (!File.Exists(jsonPath)) jsonPath = Path.Combine(Environment.GetEnvironmentVariable("AppData"), infoPath);

            if (!File.Exists(jsonPath)) throw new Exception("Dropbox could not be found!");

            var dropboxPath = File.ReadAllText(jsonPath).Split('\"')[5].Replace(@"\\", @"\");

            var dataFilePath = Path.Combine(dropboxPath, "Data");
            if (!Directory.Exists(dataFilePath)) Directory.CreateDirectory(dataFilePath);

            dataFilePath = Path.Combine(dataFilePath, "InveSim.json");

            var sim = new Simulator.Simulator();

            sim.OnNeedData += Sim_OnNeedData;
            sim.OnLog += Sim_OnLog;

            //var data = System.IO.File.Exists("data.xml") ? System.IO.File.ReadAllText("data.xml") : null;
            //sim.LoadData(data);

            var data = System.IO.File.Exists(dataFilePath) ? System.IO.File.ReadAllText(dataFilePath) : null;
            System.IO.File.WriteAllText("data.backup.json", data);
            sim.LoadJsonData(data);

            while (true)
            {

                Console.WriteLine("InveSim");
                Console.WriteLine();

                var choice = -1;

                while (choice < 0)
                {
                    Console.WriteLine("1 - Simulate vecko");
                    Console.WriteLine("2 - Simulate signallista");
                    Console.WriteLine("3 - Simulate signalllist, only high volume");
                    Console.WriteLine("4 - Update values");
                    Console.WriteLine("0 - Exit");
                    var s = Console.ReadLine();
                    choice = int.TryParse(s, out var x) ? x : -1;
                }

                if (choice == 0) return;

                sim.Setup(choice == 4);

                sim.Holdings.Clear();

                sim.Data.Signals.Signals.Clear();

                var vecko = choice == 1 || choice == 4;
                var signal = choice == 2 || choice == 3 || choice == 4;

                var sig = sim.Data.Signals;

                if (vecko)
                {

                    // V19 2020-05-04
                    sig.Add("ASSA B  ", "20200504", "20200529");
                    sig.Add("ICA     ", "20200504", "20200624");
                    sig.Add("NVP     ", "20200504", "20200515");
                    sig.Add("ATCO A  ", "20200505", "20200522");
                    sig.Add("INVE B  ", "20200505", "20200515");
                    sig.Add("RECI B  ", "20200505", "20200515");
                    sig.Add("AKEL D  ", "20200507", "20200514");
                    sig.Add("AXFO    ", "20200507", null);
                    sig.Add("TELIA   ", "20200507", "20200515");
                    sig.Add("INTRUM  ", "20200508", "20200515");

                    // V20 2020-05-11
                    sig.Add("KLED    ", "20200513", "20200604");
                    sig.Add("AEC     ", "20200514", "20200526");
                    sig.Add("BULTEN  ", "20200514", "20200529");
                    sig.Add("COMBI   ", "20200514", "20200528");
                    sig.Add("HIQ     ", "20200514", "20200601");
                    sig.Add("HUSQ B  ", "20200514", "20200520");
                    sig.Add("LIAB    ", "20200514", "20200527");
                    sig.Add("OP      ", "20200514", "20200625");
                    sig.Add("PNDX B  ", "20200514", "20200526");
                    sig.Add("RATO B  ", "20200514", "20200527");
                    sig.Add("SHOT    ", "20200514", "20200528");
                    sig.Add("TREL B  ", "20200514", "20200527");
                    sig.Add("ESSITY B", "20200515", "20200608");
                    sig.Add("GETI B  ", "20200515", "20200608");

                    // V21 2020-05-18
                    sig.Add("ISR     ", "20200518", "20200615");
                    sig.Add("PAPI    ", "20200519", null);
                    sig.Add("ARJO B  ", "20200522", "20200616");
                    sig.Add("OASM    ", "20200522", "20200612");

                    // V22 2020-05-25
                    sig.Add("ENERS   ", "20200525", "20200623");
                    sig.Add("RNBS    ", "20200525", "20200603");
                    sig.Add("GCOR    ", "20200527", "20200617");
                    sig.Add("CLEM    ", "20200528", null);

                    // V23 2020-06-01
                    sig.Add("BUSER   ", "20200602", null);
                    sig.Add("SAXG    ", "20200604", null);

                    // V24 2020-06-08
                    sig.Add("ACARIX  ", "20200612", null);
                    sig.Add("ASSA B  ", "20200612", null);
                    sig.Add("ATCO B  ", "20200612", "20200618");
                    sig.Add("ATT     ", "20200612", null);
                    sig.Add("BALD B  ", "20200612", null);
                    sig.Add("ERIC B  ", "20200612", null);
                    sig.Add("FING B  ", "20200612", null);
                    sig.Add("INVE B  ", "20200612", "20200618");
                    sig.Add("SEB A   ", "20200612", null);
                    sig.Add("SKA B   ", "20200612", null);
                    sig.Add("TELIA   ", "20200612", "20200622");

                    // V25 2020-06-15
                    sig.Add("AAC     ", "20200615", null);
                    sig.Add("DUST    ", "20200615", null);
                    sig.Add("INDU C  ", "20200615", null);
                    sig.Add("OASM    ", "20200615", "20200629");
                    sig.Add("SAAB B  ", "20200615", null);

                    // V26 2020-06-22
                    sig.Add("CTM     ", "20200623", null);
                    sig.Add("HOFI    ", "20200623", null);
                    sig.Add("NDA SE  ", "20200623", "20200629");
                    sig.Add("SHOT    ", "20200623", null);
                    sig.Add("EPRO B  ", "20200624", null);
                    sig.Add("AZN     ", "20200625", null);
                    sig.Add("SWED A  ", "20200625", null);
                    sig.Add("FABG    ", "20200625", null);
                    sig.Add("SKIS B  ", "20200625", null);
                    sig.Add("BILI A  ", "20200625", null);
                    sig.Add("COLL    ", "20200625", null);
                    sig.Add("GENO    ", "20200625", null);
                    sig.Add("PREC    ", "20200625", null);

                    // V27 2020-06-29
                    sig.Add("ADDV B  ", "20200629", null);
                    sig.Add("HM B    ", "20200629", null);
                    sig.Add("OASM    ", "20200630", null);


                }

                if (signal)
                {

                    var onlyHighVolume = choice == 3;

                    if (!onlyHighVolume)
                    {

                        sig.Add("HIFA B  ", "20200515", "20200522");
                        sig.Add("CTT     ", "20200519", "20200520");
                        sig.Add("SJR B   ", "20200525", "20200612");
                        sig.Add("CAT B   ", "20200526", "20200610");
                        sig.Add("CTT     ", "20200527", "20200528");
                        sig.Add("ACTI    ", "20200528", "20200602");
                        sig.Add("CTT     ", "20200529", "20200612");
                        sig.Add("HIFA B  ", "20200608", "20200612");
                        sig.Add("ACTI    ", "20200611", "20200622");
                        sig.Add("CAT B   ", "20200611", "20200612");
                        sig.Add("SJR B   ", "20200617", null);
                        sig.Add("CTT     ", "20200618", null);
                        sig.Add("HIFA B  ", "20200626", null);

                    }

                    sig.Add("NETI B  ", "20200518", "20200610");
                    sig.Add("ATRLJ B ", "20200519", "20200612");
                    sig.Add("FING B  ", "20200519", "20200610");
                    sig.Add("EOLU B  ", "20200520", "20200612");
                    sig.Add("WIHL    ", "20200520", "20200612");
                    sig.Add("AAK     ", "20200522", "20200609");
                    sig.Add("NEWA B  ", "20200527", "20200612");
                    sig.Add("GCOR    ", "20200601", "20200612");
                    sig.Add("AAK     ", "20200611", "20200612");
                    sig.Add("NETI B  ", "20200611", null);
                    sig.Add("WIHL    ", "20200617", "20200624");
                    sig.Add("ATRLJ B ", "20200618", "20200623");
                    sig.Add("AZA     ", "20200629", null);
                    sig.Add("FING B  ", "20200629", "20200630");


                }

                sim.Data.DateHandler.Holidays.Clear();
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 5, 1));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 5, 21));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 6, 19));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 10, 30));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 12, 24));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 12, 25));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 12, 31));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2021, 1, 1));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2021, 1, 6));

                Console.WriteLine(sim.Details());

                var until = DateTime.Today;

                var timeCheck = choice == 4 ? 1730 : 1745;

                if (int.Parse(DateTime.Now.ToString("HHmm")) < timeCheck) until = sim.Data.DateHandler.GetPreviousDate(until);

                until = sim.Data.DateHandler.GetPreviousDate(until.AddDays(1));

                var log = new List<(DateTime date, decimal cash, decimal total)>();

                //sim.Data.Symbols.Clear();

                while (sim.CurrentDate < until)
                {
                    sim.NextDay();
                    log.Add((sim.CurrentDate, sim.Cash, sim.TotalValue));
                    //data = sim.GetData();
                    //System.IO.File.WriteAllText("data.xml", data);
                    data = sim.GetJsonData();
                    System.IO.File.WriteAllText(dataFilePath, data);
                    Console.WriteLine();
                    Console.WriteLine(sim.Details());
                    //Console.WriteLine("Press enter...");
                    //Console.ReadLine();
                }

                Console.WriteLine();
                Console.ReadLine();

                foreach ((var date, decimal cash, decimal total) in log)
                {
                    Console.WriteLine($"{date:yyyy-MM-dd};{cash};{total - cash}");
                }

                Console.ReadLine();

            }

        }

        private static void Sim_OnLog(object sender, Simulator.Simulator.LogEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private static void Sim_OnNeedData(object sender, Simulator.Simulator.NeedDataEventArgs e)
        {
            
            decimal? val = null;

            //while (string.IsNullOrEmpty(Token.Cookie) || string.IsNullOrEmpty(Token.Crumb))
            //{
            //    Token.RefreshAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            //}

            //var sym = e.Symbol.Replace(" ", "-") + ".ST";

            //var data = Historical.GetPriceAsync(sym, e.Date.Date.AddDays(-1), e.Date.Date.AddDays(1)).ConfigureAwait(false).GetAwaiter().GetResult();

            //if (!data.Any())
            //{
            //    Token.RefreshAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            //    data = Historical.GetPriceAsync(sym, e.Date.Date.AddDays(-1), e.Date.Date.AddDays(1)).ConfigureAwait(false).GetAwaiter().GetResult();
            //}

            //var p = data.FirstOrDefault(d => d.Date.Equals(e.Date.Date));

            //if (p != null)
            //{
            //    switch (e.Point.ToLower())
            //    {
            //        case "c": val = (decimal)p.Close; break;
            //        case "o": val = (decimal)p.Open; break;
            //    }
            //}

            //var adt = data.Average(d => d.Volume);

            //var tickSize = GetTickSize(val.Value, (int)Math.Round(adt));

            //val = Math.Round(val.Value / tickSize) * tickSize;

            while (!val.HasValue)
            {
                Console.Write($"{e.Message}: ");
                var str = Console.ReadLine();
                if (decimal.TryParse(str, out var x)) val = x;
            }
            e.Data = val.Value;
        }

        private static decimal GetTickSize(decimal value, int avgTransDay)
        {
        
            var limits = new[] { 0.1, 0.2, 0.5, 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000, 20000, 50000 };

            var row = limits.Count(l => (decimal)l <= value);

            //row -= 1;

            var sizes = new[] { 0.0005, 0.001, 0.002, 0.005, 0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1, 2, 5, 10, 20, 50, 100, 200, 500 };

            if (row >= sizes.Length) row = sizes.Length - 1;

            return (decimal)sizes[row];

        }

    }

}
