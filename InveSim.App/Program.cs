﻿using Newtonsoft.Json;
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
using Microsoft.Office.Interop;

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
            var templatePath = Path.Combine(signalsPath, "SignalChart.xlsx");
            var symbolsPath = Path.Combine(signalsPath, "Symbols.txt");
            var currentlyOpenPath = Path.Combine(signalsPath, "CurrentlyOpen.json");
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
                    Console.WriteLine("1 - Simulate v");
                    Console.WriteLine("2 - Simulate s");
                    Console.WriteLine("3 - Simulate s, ohv");
                    Console.WriteLine("4 - Update values");
                    Console.WriteLine("5 - Generate signals");
                    Console.WriteLine("6 - Get signal data");
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

                if (choice == 6)
                {

                    Console.Write("Symbol: ");
                    var symbol = Console.ReadLine();

                    var oldFile = System.IO.File.ReadAllText(currentlyOpenPath);
                    var currentlyOpen = JsonDeserialize<List<(DateTime, string)>>(oldFile);
                    var gen = new SignalGenerator { Symbol = symbol };
                    var sym = currentlyOpen.Where(i => i.Item2 == symbol).Select(i => (DateTime?)i.Item1).FirstOrDefault();
                    if (sym.HasValue)
                    {
                        var dte = sim.Data.DateHandler.GetPreviousDate(sym.Value);
                        gen.ForceBuy = dte;
                    }
                    var x = gen.GenerateSignals();

                    Console.WriteLine("Date;Open;High;Low;Close;BuyLine;SellLine;StopLoss;In");
                    var pb = gen.Days[0].In;

                    foreach(var day in gen.Days)
                    {
                        var sl = day.StopLoss <= 0 ? "" : day.StopLoss.ToString("###0.00");
                        var inn = day.In == pb ? "" : (day.In ? "1" : "0");

                        pb = day.In;

                        Console.WriteLine($"{day.Date:yyyy-MM-dd};{day.Open:###0.00};{day.High:###0.00};{day.Low:###0.00};{day.Close:###0.00};{day.BuyLine:###0.00};{day.SellLine:###0.00};{sl};{inn}");
                    }

                    while (true)
                    {

                        Console.WriteLine();
                        Console.Write("Create chart (Y/N): ");

                        if (Console.ReadLine().ToLowerInvariant() == "y")
                        {

                            DateTime? startDate = null;
                            var def = DateTime.Today.AddMonths(-3).ToString("yyyyMMdd");
                            while (!startDate.HasValue)
                            {
                                Console.Write($"Enter start date (yyyyMMdd) [{def}]: ");
                                var inp = Console.ReadLine();
                                if (inp == "") inp = def;
                                if (DateTime.TryParseExact(inp, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var v))
                                {
                                    startDate = v;
                                }
                            }

                            DateTime? endDate = null;
                            def = DateTime.Today.ToString("yyyyMMdd");
                            if(DateTime.Now.Hour < 17) def = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");
                            while (!endDate.HasValue)
                            {
                                Console.Write($"Enter end date (yyyyMMdd) [{def}]: ");
                                var inp = Console.ReadLine();
                                if (inp == "") inp = def;
                                if (DateTime.TryParseExact(inp, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var v))
                                {
                                    endDate = v;
                                }
                            }

                            var activeRange = gen.Days.Where(d => d.Date >= startDate && d.Date <= endDate).ToList();

                            CreateChart(activeRange, symbol, templatePath);

                        }
                        else
                        {
                            break;
                        }

                    }

                    continue;

                }
                if (choice == 5)
                {

                    try
                    {

                        sig.Signals.Clear();

                        var symbolList = System.IO.File.ReadAllText(symbolsPath).Replace("\n", "").Split('\r').Where(s => !string.IsNullOrEmpty(s)).ToList();

                        var oldFile = System.IO.File.ReadAllText(currentlyOpenPath);
                        var currentlyOpen = JsonDeserialize<List<(DateTime, string)>>(oldFile);

                        var TargetSl = new List<(string, double, double)>();

                        foreach (var s in symbolList)
                        {
                            Console.WriteLine(s);

                            var gen = new SignalGenerator { Symbol = s };
                            var sym = currentlyOpen.Where(i => i.Item2 == s).Select(i => (DateTime?)i.Item1).FirstOrDefault();
                            if (sym.HasValue)
                            {
                                var dte = sim.Data.DateHandler.GetPreviousDate(sym.Value);
                                gen.ForceBuy = dte;
                            }
                            var x = gen.GenerateSignals();
                            foreach (var d in x.Item1)
                            {
                                sig.AddSignal(d, s, true, false);
                            }
                            foreach (var d in x.Item2)
                            {
                                sig.AddSignal(d, s, false, true);
                            }
                            TargetSl.Add((s, gen.Target, gen.StopLoss));
                        }

                        foreach (var s in sig.Signals)
                        {
                            s.Date = sim.Data.DateHandler.GetNextDate(s.Date);
                        }

                        var active = new List<(DateTime, string)>();

                        foreach (var symbol in sig.Signals.Select(s => s.Symbol).Distinct())
                        {
                            var sym = sig.Signals.Where(s => s.Symbol == symbol).OrderByDescending(s => s.Date).First();
                            if (sym.Buy) active.Add((sym.Date, sym.Symbol));
                        }

                        var dt = sig.Signals.Max(s => s.Date);

                        var dates = new List<DateTime>();

                        while (dates.Count < 5)
                        {
                            dates.Add(dt);
                            dt = sig.Signals.Where(s => s.Date < dt).Max(s => s.Date);
                        }

                        //if(!autoSignals) Console.ReadLine();

                        var sb = new System.Text.StringBuilder();

                        foreach (var d in dates.OrderBy(d => d))
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

                        sb.AppendLine($"Open");
                        Console.WriteLine($"Open");

                        var fileContent = JsonSerialize(active);
                        if (autoSignals) System.IO.File.WriteAllText(currentlyOpenPath, fileContent);

                        foreach (var (date, sym) in active.OrderBy(s => s.Item1))
                        {
                            var TS = TargetSl.Where(t => t.Item1 == sym).First();
                            sb.AppendLine($"{date:yyyy-MM-dd} {sym} Target: {TS.Item2:####0.00} StopLoss: {TS.Item3:####0.00}");
                            Console.WriteLine($"{date:yyyy-MM-dd} {sym} Target: {TS.Item2:####0.00} StopLoss: {TS.Item3:####0.00}");
                        }

                        if (autoSignals) System.IO.File.WriteAllText(signalsPath, sb.ToString());

                        if (autoSignals) return;

                        Console.ReadLine();

                        continue;

                    }catch(Exception ex)
                    {
                      
                        if (!autoSignals) throw; 
                                                    
                        System.IO.File.WriteAllText(signalsPath + ".error.txt", ex.ToString());

                        return;
                        
                    }

                }

                if (vecko)
                {

                    AddSignals(sig);

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
                sim.Data.DateHandler.Holidays.Add(new DateTime(2021, 4, 6));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2021, 5, 13));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2021, 6, 25));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2021, 12, 24));
                sim.Data.DateHandler.Holidays.Add(new DateTime(2021, 12, 31));

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

        private static void AddSignals(Simulator.SignalDataHolder sig)
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
            //sig.Add("SHB A   ", "20200721", null);
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
            //sig.Add("GCOR    ", "20200902", null);
            sig.Add("OBAB    ", "20200902", "20201012");
            sig.Add("BMAX    ", "20200903", "20200929");
            sig.Add("IPCO    ", "20200903", "20200922");
            sig.Add("BETS B  ", "20200904", "20200922");
            sig.Add("EPIS B  ", "20200904", "20200915");
            sig.Add("PEAB B  ", "20200904", "20200911");

            // V37 2020-09-07
            sig.Add("NIBE B  ", "20200909", "20201022");

            // V380 2020-09-14
            sig.Add("BAYN    ", "20200915", "20200924");
            sig.Add("COMBI   ", "20200915", "20201013");
            sig.Add("HIQ     ", "20200915", "20200921");

            // V39 2020-09-21
            sig.Add("OV      ", "20200922", "20201026");
            sig.Add("SSAB A  ", "20200922", "20201006");
            sig.Add("TEL2 B  ", "20200922", "20201002");
            sig.Add("CTM     ", "20200923", "20200930");

            // V40 2020-09-28
            //sig.Add("EXPRS2  ", "20200929", null);
            sig.Add("INVAJO  ", "20200930", "20201029");
            sig.Add("COPP B  ", "20201001", "20201014");
            sig.Add("FING B  ", "20201002", "20201016");

            // V41 2020-10-05
            sig.Add("AXFO    ", "20201008", "20201014");
            sig.Add("EPIS B  ", "20201008", "20201027");
            sig.Add("MYFC    ", "20201009", "20201022");

            // V42 2020-10-12
            sig.Add("AZN     ", "20201015", "20201022");
            sig.Add("ANOT    ", "20201016", "20201029");
            sig.Add("AROC    ", "20201016", "20201022");
            sig.Add("GUNN    ", "20201016", "20201202"); //Avnotering 25,00kr

            // V43 2020-10-19
            sig.Add("ALELIO  ", "20201020", "20201028");
            sig.Add("CLA B   ", "20201020", "20201026");
            sig.Add("SNM     ", "20201020", "20201026");
            sig.Add("BILL    ", "20201021", "20201027");
            sig.Add("MTRS    ", "20201021", "20201029");
            sig.Add("SKIS B  ", "20201021", "20201027");
            sig.Add("SWMA    ", "20201021", "20201030");
            sig.Add("ATCO B  ", "20201022", "20201029");
            sig.Add("BALD B  ", "20201022", "20201028");
            sig.Add("BONAV B ", "20201022", "20201217");
            sig.Add("CAST    ", "20201022", "20201028");
            sig.Add("CE      ", "20201022", "20201028");
            sig.Add("FING B  ", "20201022", "20201130");
            sig.Add("LUNE    ", "20201022", "20201029");
            sig.Add("NDA SE  ", "20201022", "20201029");
            sig.Add("SKA B   ", "20201022", "20201028");
            sig.Add("VICO    ", "20201022", "20201028");

            // V44 2020-10-26
            sig.Add("EOLU B  ", "20201026", "20201030");
            sig.Add("HIQ     ", "20201026", "20201116"); //Avnotering 2020-11-13 72,00kr
            sig.Add("FABG    ", "20201027", "20201110");
            sig.Add("LATO B  ", "20201027", "20201104");
            sig.Add("NET B   ", "20201027", "20201111");
            sig.Add("INTRUM  ", "20201028", "20201111");
            sig.Add("ARCT    ", "20201029", "20210125");
            sig.Add("EVO     ", "20201029", "20201111");
            sig.Add("SNM     ", "20201030", "20210115");

            // V45 2020-11-02
            sig.Add("CLA B   ", "20201102", "20201201");

            // V46 2020-11-09

            // V47 2020-11-16
            sig.Add("HNSA    ", "20201118", "20201124");

            // V48 2020-11-23
            sig.Add("INTRUM  ", "20201123", "20201210");
            //sig.Add("KLOV B  ", "20201124", null);
            sig.Add("LATO B  ", "20201124", "20201207");
            sig.Add("ABB     ", "20201125", "20201222");
            sig.Add("BINV    ", "20201125", "20201203");
            sig.Add("ASSA B  ", "20201126", "20201202");

            // V49 2020-11-30
            sig.Add("BALD B  ", "20201201", "20201222");
            sig.Add("SAAB B  ", "20201201", "20201207");
            sig.Add("VOLV B  ", "20201203", "20201211");

            // V50 2020-12-07
            sig.Add("MEKO    ", "20201207", "20210108");
            sig.Add("SBB D   ", "20201208", "20200107");
            sig.Add("BRIG    ", "20201210", "20201222");

            // V51 2020-12-14
            sig.Add("BOL     ", "20201214", "20210105");
            sig.Add("HOFI    ", "20201215", "20201230");
            sig.Add("NOKIA SEK", "20201215", "20201222");
            sig.Add("SHB A   ", "20201215", "20210115");
            sig.Add("ACARIX  ", "20201216", "20201229");
            sig.Add("ELUX B  ", "20201218", "20210108");

            // V52 2020-12-21
            sig.Add("FING B  ", "20201221", "20210104");
            sig.Add("NDA SE  ", "20201221", "20210201");
            sig.Add("TELIA   ", "20201222", "20210121");

            // V53 2020-12-28
            sig.Add("AOI     ", "20201230", "20210111");

            // V1 2021-01-04

            sig.Add("OP      ", "20210104", null);
            sig.Add("BETS B  ", "20210104", null);

            // V2 2021-01-11
            sig.Add("CLA B   ", "20210112", "20210301");
            sig.Add("KNOW    ", "20210112", "20210118");
            sig.Add("SOBI    ", "20210113", "20210209");

            // V3 2021-01-18

            // V4 2021-01-25
            sig.Add("BRAV    ", "20210126", "20210215");
            sig.Add("BULTEN  ", "20210126", "20210203");
            sig.Add("LUNE    ", "20210126", "20210209");
            sig.Add("TANGI   ", "20210126", "20210201");
            sig.Add("AZA     ", "20210127", "20210203");
            sig.Add("ALFA    ", "20210128", "20210205");
            sig.Add("NASO    ", "20210128", "20210401");
            sig.Add("SPEQT   ", "20210128", "20210303");

            // V5 2021-02-01

            // V6 2021-02-08
            sig.Add("ABB     ", "20210208", "20210309");
            sig.Add("TELIA   ", "20210209", "20210218");
            sig.Add("BINV    ", "20210210", "20210219");
            sig.Add("AERO    ", "20210211", null);

            // V7 2021-02-15
            sig.Add("EQT     ", "20210217", "20210223");
            sig.Add("FING B  ", "20210217", "20210223");
            sig.Add("ICA     ", "20210217", "20210324");
            sig.Add("LUG     ", "20210218", "20210318");
            sig.Add("NEWTON  ", "20210218", "20210225");
            sig.Add("NOKIA SEK", "20210218", null);
            sig.Add("SKA B   ", "20210218", "20210316");
            sig.Add("BOOZT   ", "20210219", "20210308");
            sig.Add("CAST    ", "20210219", "20210312");
            sig.Add("PCELL   ", "20210219", "20210226");

            //V8 2021-02-22
            sig.Add("ORES    ", "20210222", "20210303");
            sig.Add("ESSITY B", "20210222", "20210317");
            sig.Add("ENZY    ", "20210225", "20210308");

            //V9 2021-03-01
            sig.Add("TERRNT B", "20210305", null);

            //V10 2021-03-08
            sig.Add("AAC     ", "20210308", "20210324");
            sig.Add("ATCO A  ", "20210308", "20210312");
            sig.Add("ATCO B  ", "20210308", "20210312");
            sig.Add("ERIC B  ", "20210308", "20210312");
            sig.Add("EVO     ", "20210308", "20210312");
            sig.Add("TOBII   ", "20210308", null);
            sig.Add("NXTMS   ", "20210310", "20210322");

            //V11 2021-03-15
            sig.Add("RNBS    ", "20210315", "20210324");
            sig.Add("AOI     ", "20210318", "20210326");
            sig.Add("HOFI    ", "20210319", "20210325");

            //V12 2021-03-22
            sig.Add("BOL     ", "20210324", null);
            sig.Add("RATO B  ", "20210324", null);
            sig.Add("EOLU B  ", "20210325", null);
            sig.Add("MAV     ", "20210325", null);

            //V13 2021-03-29
            sig.Add("SHOT    ", "20210329", null);
            sig.Add("BONAV B ", "20210330", null);
            sig.Add("BULTEN  ", "20210330", null);
            sig.Add("OBAB    ", "20210330", null);

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

        public static string JsonSerialize(object o)
        {
            var serializer = new JsonSerializer
            {
                //serializer.Formatting = Formatting.Indented;
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyMMdd"
            };

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, o);
                return writer.ToString();
            }
        }

        public static T JsonDeserialize<T>(string serialized) where T: new()
        {
            if (string.IsNullOrWhiteSpace(serialized)) return new T();
            var serializer = new JsonSerializer
            {
                //serializer.Formatting = Formatting.Indented;
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyMMdd"
            };
            using (var tr = new StringReader(serialized))
            using (var reader = new JsonTextReader(tr))
            {
                return (T)serializer.Deserialize<T>(reader);
            }
        }

        private static void CreateChart(List<SignalGenerator.Day> data, string symbol, string templatePath)
        {

            Console.WriteLine("Generating...");

            var app = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbooks books = null;
            Microsoft.Office.Interop.Excel.Workbook book = null;

            try
            {

                books = app.Workbooks;
                book = books.Open(templatePath);

                app.Calculation = Microsoft.Office.Interop.Excel.XlCalculation.xlCalculationManual;
                app.ScreenUpdating = false;

                Microsoft.Office.Interop.Excel.Chart sheetG = book.Charts[1];
                Microsoft.Office.Interop.Excel.Worksheet sheet = book.Worksheets[1];

                sheet.Activate();

                var mindate = data.Min(d => d.Date);
                var maxdate = data.Max(d => d.Date);
                var lowest = data.Min(d => d.LowestValue());
                var highest = data.Max(d => d.HighestValue());

                var range = highest - lowest;

                lowest -= (range * 0.05);
                highest += (range * 0.05);

                if (lowest < 0) lowest = 0;

                var title = $"{symbol.ToUpper()}: {mindate:yyyy-MM-dd} - {maxdate:yyyy-Mm-dd}";

                ((Microsoft.Office.Interop.Excel.Range)sheet.Cells[1, 10]).Value = title;

                var row = 1;
                var isin = data[0].In;
                var p = 0;

                foreach(var day in data)
                {
                    var np = row * 100 / data.Count;
                    if(np != p)
                    {
                        Console.WriteLine($"{np}%");
                        p = np;
                    }
                    row++;
                    ((Microsoft.Office.Interop.Excel.Range)sheet.Cells[row, 1]).Value = day.Date;
                    ((Microsoft.Office.Interop.Excel.Range)sheet.Cells[row, 2]).Value = day.Open;
                    ((Microsoft.Office.Interop.Excel.Range)sheet.Cells[row, 3]).Value = day.High;
                    ((Microsoft.Office.Interop.Excel.Range)sheet.Cells[row, 4]).Value = day.Low;
                    ((Microsoft.Office.Interop.Excel.Range)sheet.Cells[row, 5]).Value = day.Close;
                    ((Microsoft.Office.Interop.Excel.Range)sheet.Cells[row, 6]).Value = day.BuyLine;
                    ((Microsoft.Office.Interop.Excel.Range)sheet.Cells[row, 7]).Value = day.SellLine;
                    if(day.StopLoss > 0) ((Microsoft.Office.Interop.Excel.Range)sheet.Cells[row, 8]).Value = day.StopLoss;
                    if (day.In != isin)
                    {
                        ((Microsoft.Office.Interop.Excel.Range)sheet.Cells[row, 9]).Value = day.In ? 1 : 0;
                        isin = day.In;
                    }
                }

                sheetG.Activate();

                Console.WriteLine("Done");

                app.Calculation = Microsoft.Office.Interop.Excel.XlCalculation.xlCalculationAutomatic;
                app.ScreenUpdating = true;

                var axis = sheetG.Axes(Microsoft.Office.Interop.Excel.XlAxisType.xlValue, Microsoft.Office.Interop.Excel.XlAxisGroup.xlPrimary) as Microsoft.Office.Interop.Excel.Axis;
                lowest -= lowest % axis.MajorUnit;
                highest += axis.MajorUnit;
                highest -= highest % axis.MajorUnit;
                axis.MinimumScale = lowest;
                axis.MaximumScale = highest;

                app.Visible = true;

                Console.WriteLine("Press any key when ready");
                Console.ReadKey();

                            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            try
            {
                if (book != null) book.Close(false);
                if (books != null) books.Close();
                app.Quit();     
                for(var i = 0; i < 10; i++)
                {
                    GC.Collect();
                }
            }
            catch { }

        }

    }

}
