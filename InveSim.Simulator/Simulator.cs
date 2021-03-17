using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace InveSim.Simulator
{

    public class Simulator
    {

        public DataHolder Data = new DataHolder();
        public DateTime CurrentDate;
        public decimal Cash;
        public decimal OriginalCash;
        public List<Holding> Holdings = new List<Holding>();
        public decimal BuyForFactor;
        public decimal BuyForMargin;
        public decimal Courtage;
        public int DaysInTrade = 0;
        public decimal TotalBuyFor = 0;
        public decimal TotalSellFor = 0;
        public DateTime StartDate;

        public void Setup(bool big)
        {
            CurrentDate = new DateTime(2020, 4, 30);
            StartDate = CurrentDate;
            Cash = big ? 1000000 : 10000;
            OriginalCash = Cash;
            BuyForFactor = big ? 0.001M : 0.1M;
            BuyForMargin = 0.1M;
            Courtage = 0.0025M;
            DaysInTrade = 0;
            TotalBuyFor = 0;
            TotalSellFor = 0;
        }

        public void LoadData(string data) => Data = DataHolder.Deserialize(data);

        public string GetData() => Data.Serialize();

        public void LoadJsonData(string data) => Data = DataHolder.JsonDeserialize(data);

        public string GetJsonData() => Data.JsonSerialize();

        public decimal TotalValue => Cash + Holdings.Sum(h => h.Count * h.CurrentPrice);

        public string Details()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Date: {CurrentDate}");
            if (Holdings.Any())
            {
                sb.AppendLine("Holdings:");

                foreach (var holding in Holdings.OrderBy(h => h.Symbol))
                {
                    sb.AppendLine(holding.Details());
                }
                sb.AppendLine();
            }
            sb.AppendLine($"Cash: {Cash}");
            sb.AppendLine($"Total: {TotalValue}, Result: {TotalValue-OriginalCash} ({(TotalValue/OriginalCash-1)*100:###0.00}%)");
            if (DaysInTrade > 0)
            {
                var PL = TotalSellFor - TotalBuyFor;
                var PLP = (TotalSellFor * 100 / TotalBuyFor) - 100;
                sb.AppendLine($"DaysInTrades: {DaysInTrade}, Profit:{PL:######0.00} ({PLP:###0.00}%) , Profit/day: {PLP / DaysInTrade:###0.00}%");
            }

            var days = CurrentDate.Subtract(StartDate).TotalDays;

            if (days > 0)
            {
                var factor = TotalValue / OriginalCash;

                var perDay = (Math.Pow((double)factor, 1d / days)) * 100 - 100;
                var perWeek = (Math.Pow((double)factor, 7d / days)) * 100 - 100;
                var perMonth = (Math.Pow((double)factor, (365d / 12d) / days)) * 100 - 100;
                var perYear = (Math.Pow((double)factor, 365d / days)) * 100 - 100;

                sb.AppendLine($"{perDay:###0.00}%/d, {perWeek:###0.00}%/w, {perMonth:###0.00}%/m, {perYear:###0.00}%/y");
            }

            return sb.ToString();
        }

        public void NextDay()
        {

            var totalValue = TotalValue;
            var buyFor = TotalValue * BuyForFactor;
            var buyForWithMargin = buyFor * (1 + BuyForMargin);

            var lastDate = CurrentDate;
            CurrentDate = Data.DateHandler.GetNextDate(CurrentDate);

            var signals = Data.Signals.GetSignals(CurrentDate);

            foreach (var signal in signals.Where(s => s.Buy))
            {
                var sym = Data.GetSymbol(signal.Symbol);
                if (!sym.GetClose(lastDate).HasValue) sym.SetClose(lastDate, NeedData($"Provide CLOSE for {signal.Symbol} for date {lastDate}", signal.Symbol, "C", lastDate));
                if (!sym.GetOpen(CurrentDate).HasValue) sym.SetOpen(CurrentDate, NeedData($"Provide OPEN for {signal.Symbol} for date {CurrentDate}", signal.Symbol, "O", CurrentDate));
                if (!sym.GetClose(CurrentDate).HasValue) sym.SetClose(CurrentDate, NeedData($"Provide CLOSE for {signal.Symbol} for date {CurrentDate}", signal.Symbol, "C", CurrentDate));
            }

            foreach (var signal in signals.Where(s => s.Sell))
            {
                var sym = Data.GetSymbol(signal.Symbol);
                if (!sym.GetOpen(CurrentDate).HasValue) sym.SetOpen(CurrentDate, NeedData($"Provide OPEN for {signal.Symbol} for date {CurrentDate}", signal.Symbol, "O", CurrentDate));
            }

            foreach(var signal in signals.Where(s => s.Buy).OrderByDescending(s => Data.GetSymbol(s.Symbol).GetClose(lastDate)))
            {
                if(Cash >= buyForWithMargin)
                {
                    var count = Math.Floor(buyFor / Data.GetSymbol(signal.Symbol).GetClose(lastDate).Value);
                    if (count == 0)
                    {
                        Log($"Could not buy {signal.Symbol}, price/each above maximum buying sum");
                        continue;
                    }
                    Cash -= count * Data.GetSymbol(signal.Symbol).GetOpen(CurrentDate).Value * (1 + Courtage);
                    Holdings.Add(new Holding(signal.Symbol, count, Data.GetSymbol(signal.Symbol).GetOpen(CurrentDate).Value, Data.GetSymbol(signal.Symbol).GetOpen(CurrentDate).Value * (1 + Courtage), CurrentDate));
                    Log($"Bought {count} {signal.Symbol}");
                }
                else
                {
                    Log($"Could not buy {signal.Symbol}, to low cash funds");
                }
            }

            foreach(var signal in signals.Where(s => s.Sell))
            {
                var holding = Holdings.FirstOrDefault(h => h.Symbol == signal.Symbol);
                if(holding != null)
                {
                    var open = Data.GetSymbol(signal.Symbol).GetOpen(CurrentDate).Value;
                    var soldfor = holding.Count * open * (1 - Courtage);
                    var boughtFor = holding.Count * holding.GAV;
                    Cash += soldfor;
                    Holdings.Remove(holding);
                    Log($"Sold {holding.Count} {signal.Symbol} for {open}. Result: {soldfor - boughtFor:######0.00} ({(soldfor / boughtFor - 1) * 100:####0.00}%)");
                    TotalBuyFor += boughtFor;
                    TotalSellFor += soldfor;
                    DaysInTrade += (int)(CurrentDate - holding.PurchaseDate).TotalDays;
                }
                else
                {
                    Log($"Could not sell {signal.Symbol}, not holding it");
                }
            }

            foreach(var holding in Holdings)
            {
                var sym = Data.GetSymbol(holding.Symbol);
                if(!sym.GetClose(CurrentDate).HasValue) sym.SetClose(CurrentDate, NeedData($"Provide CLOSE for {holding.Symbol} for date {CurrentDate}", holding.Symbol, "C", CurrentDate));
                holding.CurrentPrice = sym.GetClose(CurrentDate).Value;
            }

        }

        public class NeedDataEventArgs : EventArgs
        {

            public string Message { get; set; }
            public decimal Data { get; set; }
            public string Symbol { get; set; }
            public string Point { get; set; }
            public DateTime Date { get; set; }

        }

        public delegate void NeedDataEventHandler(object sender, NeedDataEventArgs e);

        public event NeedDataEventHandler OnNeedData;

        protected decimal NeedData(string message, string symbol, string point, DateTime date)
        {
            var args = new NeedDataEventArgs { Message = message, Symbol = symbol, Point = point, Date = date };
            OnNeedData?.Invoke(this, args);
            return args.Data;
        }

        public class LogEventArgs : EventArgs
        {
            public string Message { get; set; }
        }

        public delegate void LogEventHandler(object sender, LogEventArgs e);

        public event LogEventHandler OnLog;

        protected void Log(string message)
        {
            var args = new LogEventArgs { Message = message };
            OnLog?.Invoke(this, args);
        }

        public class Holding
        {
            
            public string Symbol { get; set; }
            public decimal Count { get; set; }
            public decimal PurchasePrice { get; set; }
            public decimal GAV { get; set; }
            public DateTime PurchaseDate { get; set; }
            public decimal CurrentPrice { get; set; }

            public Holding(string symbol, decimal count, decimal purchasePrice, decimal gav, DateTime purchaseDate)
            {
                Symbol = symbol;
                Count = count;
                PurchasePrice = purchasePrice;
                CurrentPrice = purchasePrice;
                GAV = gav;
                PurchaseDate = purchaseDate;
            }

            public string Details()
            {
                return $"{Symbol, -10} {Count, 5} {GAV, 9:###0.0000} {CurrentPrice, 9:###0.0000} {(CurrentPrice / GAV - 1) * 100, 6:####0.00}% {Count * CurrentPrice, 10:####0.0000}";
            }

        }

    }

}
