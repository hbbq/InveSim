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

            Console.WriteLine("InveSim");
            Console.WriteLine();

            var choice = -1;

            while(choice < 0)
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

            sim.Data.Signals.Signals.Clear();

            var vecko = choice == 1 || choice == 4;
            var signal = choice == 2 || choice == 3 || choice == 4;

            if (vecko)
            {

                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 14), "RATO B", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 27), "RATO B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 14), "LIAB", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 27), "LIAB", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 4), "NVP", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 15), "NVP", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 4), "ASSA B", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 29), "ASSA B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 5), "RECI B", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 15), "RECI B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 5), "INVE B", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 15), "INVE B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 13), "KLED", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 6, 4), "KLED", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 4), "ICA", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "ICA", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 5), "ATCO A", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 22), "ATCO A", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 15), "ESSITY B", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 6, 8), "ESSITY B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 14), "HIQ", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 6, 1), "HIQ", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 14), "PNDX B", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 26), "PNDX B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 14), "AEC", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 26), "AEC", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 8), "INTRUM", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 15), "INTRUM", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 7), "TELIA", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 15), "TELIA", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 7), "AKEL D", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 14), "AKEL D", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 14), "COMBI", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 28), "COMBI", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 14), "HUSQ B", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 20), "HUSQ B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 14), "OP", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "OP", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 14), "SHOT", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 28), "SHOT", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 18), "ISR", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "ISR", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 14), "TREL B", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 27), "TREL B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 19), "PAPI", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "PAPI", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 22), "ARJO B", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "ARJO B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 22), "OASM", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "OASM", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 25), "RNBS", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 6, 3), "RNBS", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 25), "ENERS", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "ENERS", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 7), "AXFO", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "AXFO", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 14), "BULTEN", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 29), "BULTEN", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 15), "GETI B", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 6, 8), "GETI B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 27), "GCOR", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "GCOR", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 28), "CLEM", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "CLEM", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 6, 2), "BUSER", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "BUSER", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 6, 4), "SAXG", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "SAXG", false, true);

            }
            
            if(signal)
            {

                var onlyHighVolume = choice == 3;

                if (!onlyHighVolume)
                {
                    sim.Data.Signals.AddSignal(new DateTime(2020, 5, 15), "HIFA B", true, false);
                    sim.Data.Signals.AddSignal(new DateTime(2020, 5, 22), "HIFA B", false, true);
                    sim.Data.Signals.AddSignal(new DateTime(2020, 5, 19), "CTT", true, false);
                    sim.Data.Signals.AddSignal(new DateTime(2020, 5, 20), "CTT", false, true);
                    sim.Data.Signals.AddSignal(new DateTime(2020, 5, 27), "CTT", true, false);
                    sim.Data.Signals.AddSignal(new DateTime(2020, 5, 28), "CTT", false, true);
                    sim.Data.Signals.AddSignal(new DateTime(2020, 5, 29), "CTT", true, false);
                    //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "CTT", false, true);
                    sim.Data.Signals.AddSignal(new DateTime(2020, 5, 25), "SJR B", true, false);
                    //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "SJR B", false, true);
                    sim.Data.Signals.AddSignal(new DateTime(2020, 5, 26), "CAT B", true, false);
                    sim.Data.Signals.AddSignal(new DateTime(2020, 6, 10), "CAT B", false, true);
                    sim.Data.Signals.AddSignal(new DateTime(2020, 5, 28), "ACTI", true, false);
                    sim.Data.Signals.AddSignal(new DateTime(2020, 6, 2), "ACTI", false, true);
                    sim.Data.Signals.AddSignal(new DateTime(2020, 6, 8), "HIFA B", true, false);
                    //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "HIFA B", false, true);
                }

                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 18), "NETI B", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 6, 10), "NETI B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 19), "ATRLJ B", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "ATRLJ B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 19), "FING B", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 6, 10), "FING B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 20), "EOLU B", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "EOLU B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 20), "WIHL", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "WIHL", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 22), "AAK", true, false);
                sim.Data.Signals.AddSignal(new DateTime(2020, 6, 9), "AAK", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 5, 27), "NEWA B", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "NEWA B", false, true);
                sim.Data.Signals.AddSignal(new DateTime(2020, 6, 1), "GCOR", true, false);
                //sim.Data.Signals.AddSignal(new DateTime(2020, , ), "GCOR", false, true);

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

            if (int.Parse(DateTime.Now.ToString("HHmm")) < 1745) until = sim.Data.DateHandler.GetPreviousDate(until);

            var log = new List<(DateTime date, decimal cash, decimal total)>();

            while(sim.CurrentDate < until)
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
