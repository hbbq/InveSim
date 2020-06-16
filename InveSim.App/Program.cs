using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
                    sig.Add("ICA     ", "20200504", null);
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
                    sig.Add("OP      ", "20200514", null);
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
                    sig.Add("ENERS   ", "20200525", null);
                    sig.Add("RNBS    ", "20200525", "20200603");
                    sig.Add("GCOR    ", "20200527", "20200617");
                    sig.Add("CLEM    ", "20200528", null);

                    // V23 2020-06-01
                    sig.Add("BUSER   ", "20200602", null);
                    sig.Add("SAXG    ", "20200604", null);

                    // V24 2020-06-08
                    sig.Add("ACARIX  ", "20200612", null);
                    sig.Add("ASSA B  ", "20200612", null);
                    sig.Add("ATCO B  ", "20200612", null);
                    sig.Add("ATT     ", "20200612", null);
                    sig.Add("BALD B  ", "20200612", null);
                    sig.Add("ERIC B  ", "20200612", null);
                    sig.Add("FING B  ", "20200612", null);
                    sig.Add("INVE B  ", "20200612", null);
                    sig.Add("SEB A   ", "20200612", null);
                    sig.Add("SKA B   ", "20200612", null);
                    sig.Add("TELIA   ", "20200612", null);

                    // V25 2020-06-15
                    sig.Add("AAC     ", "20200615", null);
                    sig.Add("DUST    ", "20200615", null);
                    sig.Add("INDU C  ", "20200615", null);
                    sig.Add("OASM    ", "20200615", null);
                    sig.Add("SAAB B  ", "20200615", null);
                    
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
                        sig.Add("ACTI    ", "20200611", null);
                        sig.Add("CAT B   ", "20200611", "20200612");
                        sig.Add("SJR B   ", "20200617", null);

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
                    sig.Add("WIHL    ", "20200617", null);

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
                    Console.WriteLine("Press enter...");
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
            while (!val.HasValue)
            {
                Console.Write($"{e.Message}: ");
                var str = Console.ReadLine();
                if (decimal.TryParse(str, out var x)) val = x;
            }
            e.Data = val.Value;
        }
    }

}
