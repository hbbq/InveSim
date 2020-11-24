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

            //args = new[] { "autosignals" };

            //var g = new SignalGenerator { Symbol = "ACARIX" };
            //g.GenerateSignals();

            //Console.ReadLine();

            //return;

            var autoSignals = false;

            if (args.Length > 0 && args[0].ToLowerInvariant() == "autosignals") autoSignals = true;

            var infoPath = @"Dropbox\info.json";

            var jsonPath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), infoPath);

            if (!File.Exists(jsonPath)) jsonPath = Path.Combine(Environment.GetEnvironmentVariable("AppData"), infoPath);

            if (!File.Exists(jsonPath)) throw new Exception("Dropbox could not be found!");

            var dropboxPath = File.ReadAllText(jsonPath).Split('\"')[5].Replace(@"\\", @"\");

            var dataFilePath = Path.Combine(dropboxPath, "Data");
            if (!Directory.Exists(dataFilePath)) Directory.CreateDirectory(dataFilePath);

            var signalsPath = Path.Combine(dataFilePath, $"Invesim");
            if (!Directory.Exists(signalsPath)) Directory.CreateDirectory(signalsPath);
            signalsPath = Path.Combine(signalsPath, $"Signals");
            if (!Directory.Exists(signalsPath)) Directory.CreateDirectory(signalsPath);
            signalsPath = Path.Combine(signalsPath, $"{DateTime.Today:yyyy-MM-dd}.txt");

            dataFilePath = Path.Combine(dataFilePath, "InveSim.v2.json");

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

                if (autoSignals) choice = 5;

                while (choice < 0)
                {
                    Console.WriteLine("1 - Simulate vecko");
                    Console.WriteLine("2 - Simulate signallista");
                    Console.WriteLine("3 - Simulate signalllist, only high volume");
                    Console.WriteLine("4 - Update values");
                    Console.WriteLine("5 - Generate signals");
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

                if (choice == 5)
                {

                    sig.Signals.Clear();

                    var symbolList = Resource1.symbols.Split('\r').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

                    foreach (var s in symbolList)
                    {
                        Console.WriteLine(s);

                        var gen = new SignalGenerator { Symbol = s };
                        var x = gen.GenerateSignals();
                        foreach (var d in x.Item1)
                        {
                            sig.AddSignal(d, s, true, false);
                        }
                        foreach (var d in x.Item2)
                        {
                            sig.AddSignal(d, s, false, true);
                        }
                    }

                    foreach (var s in sig.Signals)
                    {
                        s.Date = sim.Data.DateHandler.GetNextDate(s.Date);
                    }


                    var dt = sig.Signals.Max(s => s.Date);

                    var dates = new List<DateTime>();

                    while(dates.Count < 5)
                    {
                        dates.Add(dt);
                        dt = sig.Signals.Where(s => s.Date < dt).Max(s => s.Date);
                    }

                    //if(!autoSignals) Console.ReadLine();

                    var sb = new System.Text.StringBuilder();

                    foreach(var d in dates.OrderBy(d => d))
                    {

                        sb.AppendLine($"Signals for {d:yyyy-MM-dd}");
                        Console.WriteLine($"Signals for {d:yyyy-MM-dd}");

                        foreach (var s in sig.Signals.Where(x => x.Date.Equals(d)))
                        {
                            sb.AppendLine($"{s.Symbol}, {(s.Buy ? "buy" : "sell")}");
                            Console.WriteLine($"{s.Symbol}, {(s.Buy ? "buy" : "sell")}");
                        }

                        sb.AppendLine();

                    }

                    if (autoSignals) System.IO.File.WriteAllText(signalsPath, sb.ToString());

                    if (autoSignals) return;

                    Console.ReadLine();

                    continue;

                }

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
                    sig.Add("AXFO    ", "20200507", "20200716");
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
                    sig.Add("PAPI    ", "20200519", "20200708");
                    sig.Add("ARJO B  ", "20200522", "20200616");
                    sig.Add("OASM    ", "20200522", "20200612");

                    // V22 2020-05-25
                    sig.Add("ENERS   ", "20200525", "20200623");
                    sig.Add("RNBS    ", "20200525", "20200603");
                    sig.Add("GCOR    ", "20200527", "20200617");
                    sig.Add("CLEM    ", "20200528", "20200701");

                    // V23 2020-06-01
                    sig.Add("BUSER   ", "20200602", "20200715");
                    sig.Add("SAXG    ", "20200604", "20200728");

                    // V24 2020-06-08
                    sig.Add("ACARIX  ", "20200612", "20200716");
                    sig.Add("ASSA B  ", "20200612", "20200716");
                    sig.Add("ATCO B  ", "20200612", "20200618");
                    sig.Add("ATT     ", "20200612", "20200713");
                    sig.Add("BALD B  ", "20200612", "20200807");
                    sig.Add("ERIC B  ", "20200612", "20200707");
                    sig.Add("FING B  ", "20200612", "20200701");
                    sig.Add("INVE B  ", "20200612", "20200618");
                    sig.Add("SEB A   ", "20200612", "20200714");
                    sig.Add("SKA B   ", "20200612", "20200731");
                    sig.Add("TELIA   ", "20200612", "20200622");

                    // V25 2020-06-15
                    sig.Add("AAC     ", "20200615", "20200713");
                    sig.Add("DUST    ", "20200615", "20200706");
                    sig.Add("INDU C  ", "20200615", "20200807");
                    sig.Add("OASM    ", "20200615", "20200629");
                    sig.Add("SAAB B  ", "20200615", "20200720");

                    // V26 2020-06-22
                    sig.Add("CTM     ", "20200623", "20200721");
                    sig.Add("HOFI    ", "20200623", "20200714");
                    sig.Add("NDA SE  ", "20200623", "20200629");
                    sig.Add("SHOT    ", "20200623", "20200723");
                    sig.Add("EPRO B  ", "20200624", "20200720");
                    sig.Add("AZN     ", "20200625", "20200716");
                    sig.Add("SWED A  ", "20200625", "20200709");
                    sig.Add("FABG    ", "20200625", "20200819");
                    sig.Add("SKIS B  ", "20200625", "20200728");
                    sig.Add("BILI A  ", "20200625", "20200720");
                    sig.Add("COLL    ", "20200625", "20200716");
                    sig.Add("GENO    ", "20200625", "20200703");
                    sig.Add("PREC    ", "20200625", "20200724");

                    // V27 2020-06-29
                    sig.Add("ADDV B  ", "20200629", "20200716");
                    sig.Add("HM B    ", "20200629", "20200811");
                    sig.Add("OASM    ", "20200630", "20200819");
                    sig.Add("SENS    ", "20200702", "20200804");

                    // V28 2020-07-06
                    sig.Add("DIOS    ", "20200710", "20200728");

                    // V29 2020-07-13
                    sig.Add("ENERS   ", "20200714", "20200916");
                    sig.Add("TOBII   ", "20200717", "20200807");

                    // V30 2020-07-20
                    sig.Add("SCA B   ", "20200720", "20200727");
                    sig.Add("GUNN    ", "20200721", "20200929");
                    sig.Add("SHB A   ", "20200721", null);
                    sig.Add("ICA     ", "20200722", "20200812");
                    sig.Add("NETI B  ", "20200722", "20200728");
                    sig.Add("SKF B   ", "20200722", "20200730");
                    sig.Add("LUC     ", "20200723", "20200810");

                    // V31 2020-07-27
                    sig.Add("DUST    ", "20200728", "20200827");
                    sig.Add("ASSA B  ", "20200731", "20200916");
                    sig.Add("CLAS B  ", "20200731", "20200810");

                    // V32 2020-08-03
                    sig.Add("ABB     ", "20200803", "20200812");
                    sig.Add("TELIA   ", "20200803", "20200901");
                    sig.Add("TREL B  ", "20200803", "20200812");
                    sig.Add("VOLV B  ", "20200803", "20200812");

                    // V33 2020-08-10
                    sig.Add("AZN     ", "20200811", "20200904");
                    sig.Add("GETI B  ", "20200811", "20200831");

                    // V34 2020-08-17
                    sig.Add("PNDX B  ", "20200818", "20200825");
                    sig.Add("SHOT    ", "20200819", "20200923");
                    sig.Add("MYFC    ", "20200820", "20201008");
                    sig.Add("OP      ", "20200821", "20201007");

                    // V35 2020-08-24
                    sig.Add("LUNE    ", "20200824", "20200904");
                    sig.Add("ENZY    ", "20200825", "20200902");

                    // V36 2020-08-31
                    sig.Add("ALFA    ", "20200901", "20200922");
                    sig.Add("BONAV B ", "20200901", "20200908");
                    sig.Add("OASM    ", "20200901", "20200918");
                    sig.Add("SAAB B  ", "20200901", "20200925");
                    sig.Add("SECU B  ", "20200901", "20200915");
                    sig.Add("AOI     ", "20200902", "20200908");
                    sig.Add("GCOR    ", "20200902", null);
                    sig.Add("OBAB    ", "20200902", "20201012");
                    sig.Add("BMAX    ", "20200903", "20200929");
                    sig.Add("IPCO    ", "20200903", "20200922");
                    sig.Add("BETS B  ", "20200904", "20200922");
                    sig.Add("EPIS B  ", "20200904", "20200915");
                    sig.Add("PEAB B  ", "20200904", "20200911");

                    // V37 2020-09-07
                    sig.Add("NIBE B  ", "20200909", "20201022");

                    // V39 2020-09-14
                    sig.Add("BAYN    ", "20200915", "20200924");
                    sig.Add("COMBI   ", "20200915", "20201013");
                    sig.Add("HIQ     ", "20200915", "20200921");

                    // V40 2020-09-21
                    sig.Add("OV      ", "20200922", "20201026");
                    sig.Add("SSAB A  ", "20200922", "20201006");
                    sig.Add("TEL2 B  ", "20200922", "20201002");
                    sig.Add("CTM     ", "20200923", "20200930");

                    // V41 2020-09-28
                    sig.Add("EXPRS2  ", "20200929", null);
                    sig.Add("INVAJO  ", "20200930", "20201029");
                    sig.Add("COPP B  ", "20201001", "20201014");
                    sig.Add("FING B  ", "20201002", "20201016");

                    // V42 2020-10-05
                    sig.Add("AXFO    ", "20201008", "20201014");
                    sig.Add("EPIS B  ", "20201008", "20201027");
                    sig.Add("MYFC    ", "20201009", "20201022");

                    // V43 2020-10-12
                    sig.Add("AZN     ", "20201015", "20201022");
                    sig.Add("ANOT    ", "20201016", "20201029");
                    sig.Add("AROC    ", "20201016", "20201022");
                    sig.Add("GUNN    ", "20201016", null);

                    // V44 2020-10-19
                    sig.Add("ALELIO  ", "20201020", "20201028");
                    sig.Add("CLA B   ", "20201020", "20201026");
                    sig.Add("SNM     ", "20201020", "20201026");
                    sig.Add("BILL    ", "20201021", "20201027");
                    sig.Add("MTRS    ", "20201021", "20201029");
                    sig.Add("SKIS B  ", "20201021", "20201027");
                    sig.Add("SWMA    ", "20201021", "20201030");
                    sig.Add("ATCO B  ", "20201022", "20201029");
                    sig.Add("BALD B  ", "20201022", "20201028");
                    sig.Add("BONAV B ", "20201022", null);
                    sig.Add("CAST    ", "20201022", "20201028");
                    sig.Add("CE      ", "20201022", "20201028");
                    sig.Add("FING B  ", "20201022", null);
                    sig.Add("LUNE    ", "20201022", "20201029");
                    sig.Add("NDA SE  ", "20201022", "20201029");
                    sig.Add("SKA B   ", "20201022", "20201028");
                    sig.Add("VICO    ", "20201022", "20201028");

                    // V45 2020-10-26
                    sig.Add("EOLU B  ", "20201026", "20201030");
                    sig.Add("HIQ     ", "20201026", "20201116"); //Avnotering 2020-11-13 72,00kr
                    sig.Add("FABG    ", "20201027", "20201110");
                    sig.Add("LATO B  ", "20201027", "20201104");
                    sig.Add("NET B   ", "20201027", "20201111");
                    sig.Add("INTRUM  ", "20201028", "20201111");
                    sig.Add("ARCT    ", "20201029", null);
                    sig.Add("EVO     ", "20201029", "20201111");
                    sig.Add("SNM     ", "20201030", null);

                    // V46 2020-11-02
                    sig.Add("CLA B   ", "20201102", null);

                    // V47 2020-11-09

                    // V48 2020-11-16
                    sig.Add("HNSA    ", "20201118", "20201124");

                    // V49 2020-11-23
                    sig.Add("INTRUM  ", "20201123", null);
                    sig.Add("KLOV B  ", "20201124", null);
                    sig.Add("LATO B  ", "20201124", null);

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
                        sig.Add("SJR B   ", "20200617", "20200711");
                        sig.Add("CTT     ", "20200618", "20200701");
                        sig.Add("HIFA B  ", "20200626", "20200707");
                        sig.Add("CAT B   ", "20200702", "20200720");
                        sig.Add("CTT     ", "20200703", null);
                        sig.Add("ACTI    ", "20200708", "20200714");
                        sig.Add("SJR B   ", "20200720", null);
                        sig.Add("ACTI    ", "20200721", null);
                        sig.Add("CAT B   ", "20200721", null);

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
                    sig.Add("NETI B  ", "20200611", "20200702");
                    sig.Add("WIHL    ", "20200617", "20200624");
                    sig.Add("ATRLJ B ", "20200618", "20200623");
                    sig.Add("AZA     ", "20200629", "20200701");
                    sig.Add("FING B  ", "20200629", "20200630");
                    sig.Add("EOLU B  ", "20200701", "20200715");
                    sig.Add("AZA     ", "20200702", null);
                    sig.Add("GCOR    ", "20200703", "20200714");
                    sig.Add("WIHL    ", "20200703", "20200708");
                    sig.Add("ATRLJ B ", "20200707", "20200708");
                    sig.Add("ATRLJ B ", "20200709", "20200713");
                    sig.Add("ATRLJ B ", "20200714", "20200720");
                    sig.Add("FING B  ", "20200714", null);
                    sig.Add("AAK     ", "20200716", "20200720");
                    sig.Add("EOLU B  ", "20200721", null);


                }

                sim.Data.DateHandler.Holidays.Clear();
                sim.Data.DateHandler.Holidays.Add(new DateTime(2019, 4, 19));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2019, 5, 30));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2019, 6, 6));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2019, 6, 10));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2019, 12, 24));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2019, 12, 25));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2019, 12, 26));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2019, 12, 31));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 1, 1));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 1, 6));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 4, 10));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 4, 13));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 5, 1));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 5, 21));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2020, 6, 19));
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

                var lastData = "";

                while (sim.CurrentDate < until)
                {
                    sim.NextDay();
                    log.Add((sim.CurrentDate, sim.Cash, sim.TotalValue));
                    //data = sim.GetData();
                    //System.IO.File.WriteAllText("data.xml", data);
                    data = sim.GetJsonData();
                    if (data != lastData)
                    {
                        for (var i = 0; i < 10; i++)
                        {
                            try
                            {
                                System.IO.File.WriteAllText(dataFilePath, data);
                                break;
                            }
                            catch (System.IO.IOException) when (i < 9)
                            {
                                Console.WriteLine("IO Error, retrying...");
                                System.Threading.Thread.Sleep(1000);
                            }
                        }
                    }
                    lastData = data;
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

                if (autoSignals) break;

            }

        }

        private static void Sim_OnLog(object sender, Simulator.Simulator.LogEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private static void Sim_OnNeedData(object sender, Simulator.Simulator.NeedDataEventArgs e)
        {
            
            decimal? val = null;

            while (string.IsNullOrEmpty(Token.Cookie) || string.IsNullOrEmpty(Token.Crumb))
            {
                Token.RefreshAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }

            var sym = e.Symbol.Replace(" ", "-") + ".ST";

            var data = Historical.GetPriceAsync(sym, e.Date.Date.AddDays(-5), e.Date.Date.AddDays(5)).ConfigureAwait(false).GetAwaiter().GetResult();

            if (!data.Any())
            {
                Token.RefreshAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                data = Historical.GetPriceAsync(sym, e.Date.Date.AddDays(-5), e.Date.Date.AddDays(5)).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            var p = data.FirstOrDefault(d => d.Date.Equals(e.Date.Date));

            if (p != null)
            {
                switch (e.Point.ToLower())
                {
                    case "c": val = (decimal)p.Close; break;
                    case "o": val = (decimal)p.Open; break;
                }
            }

            var adt = data.Average(d => d.Volume);

            if (val.HasValue)
            {

                var tickSize = GetTickSize(val.Value, (int)Math.Round(adt));

                val = Math.Round(val.Value / tickSize) * tickSize;

            }

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
