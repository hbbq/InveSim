using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YahooFinanceAPI;
using YahooFinanceAPI.Models;

namespace InveSim.App
{

    public class SignalGenerator
    {

        public string Symbol { get; set; }

        public (List<DateTime>, List<DateTime>) GenerateSignals()
        {

            var bs = new List<DateTime>();
            var ss = new List<DateTime>();

            while (string.IsNullOrEmpty(Token.Cookie) || string.IsNullOrEmpty(Token.Crumb))
            {
                Token.RefreshAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }

            var sym = Symbol.Replace(" ", "-") + ".ST";

            //var startdate = DateTime.Today.AddYears(-2);
            var startdate = new DateTime(2018, 1, 1);

            var data = Historical.GetPriceAsync(sym, startdate, DateTime.Today.AddDays(1)).ConfigureAwait(false).GetAwaiter().GetResult();

            if (!data.Any())
            {
                Token.RefreshAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                data = Historical.GetPriceAsync(sym, startdate, DateTime.Today.AddDays(1)).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            if (!data.Any()) throw new Exception($"ERROR, no data for {Symbol}");

            var lp = 8;
            var sp = 3;
            var buyLevel = 0.02;
            var sellLevel = 0.75;
            var slemal = 14;
            var emacheck = 3;
            var lfactor = 4;
            var hodlfor = 4;
            var lbbase = 0.0;

            lp *= lfactor;
            sp *= lfactor;
            emacheck *= lfactor;

            var HODL = 0;
            var sl = 0.0;

            var points = data.OrderBy(d => d.Date).ToList();

            var isIn = false;

            var count = 0;

            foreach(var point in points)
            {

                count++;

                if (count <= 60) continue;

                var toThisDay = points.Where(d => d.Date <= point.Date).ToList();

                var lows = toThisDay.Select(x => x.Low).ToList();
                var highs = toThisDay.Select(x => x.High).ToList();
                var opens = toThisDay.Select(x => x.Open).ToList();
                var closes = toThisDay.Select(x => x.Close).ToList();

                var slema = EMA(lows, slemal);

                var plema = EMA(lows, slemal, 1);

                var lmax = toThisDay.OrderByDescending(p => p.Date).Take(lp).Max(p => p.High);
                var lmin = toThisDay.OrderByDescending(p => p.Date).Take(lp).Min(p => p.Low);
                var smax = toThisDay.OrderByDescending(p => p.Date).Take(sp).Max(p => p.High);
                var smin = toThisDay.OrderByDescending(p => p.Date).Take(sp).Min(p => p.Low);
                var dmax = toThisDay.OrderByDescending(p => p.Date).Skip(sp).Take(lp - sp).Max(p => p.High);
                var dmin = toThisDay.OrderByDescending(p => p.Date).Skip(sp).Take(lp - sp).Min(p => p.Low);

                var lrange = lmax - lmin;
                var srange = smax - smin;
                var drange = dmax - dmin;

                var bbase = smin;
                var sbase = Math.Max(smax, lmax);

                var check1 = EMA(closes, emacheck) < EMA(closes, emacheck, emacheck);
                var check2 = EMA(closes, emacheck, emacheck) < EMA(closes, emacheck, emacheck * 2);

                if (check1 && check2)
                {
                   bbase = bbase + EMA(closes, emacheck) - EMA(closes, emacheck, emacheck * 2);
                   //bbase += EMA(closes, emacheck*3) - EMA(closes, emacheck, emacheck * 2);
                }

                lbbase = bbase;

                var buyline = bbase + srange * buyLevel;
                var sellline = sbase - srange + srange * sellLevel;

                var buy = smin >= lmin && srange / lrange < 0.7 && point.Close <= buyline;
                var sell = point.Close > sellline;

                if (HODL > 0) HODL--;

                //if (point.Date > new DateTime(2020, 06, 12))
                //    Console.WriteLine("x");

                var nsl = point.Open - ATR(toThisDay, 14 * lfactor);

                if (sl == -1) sl = nsl;

                var usl = Math.Max(sl, sl + slema - EMA(lows, slemal, 1));

                if (sl > 0 && HODL <= 0 && sl < usl) sl = usl;

                buy &= !sell;

                sell = sell || point.Close < sl;

                sell &= HODL <= 0;
                buy &= HODL <= 0;

                if (buy)
                {
                    if (sl == 0)
                    {
                        sl = -1;
                        HODL = hodlfor;
                    }
                    if (!isIn)
                    {
                        Console.WriteLine($"{point.Date}: buy");
                        bs.Add(point.Date);
                    }
                    isIn = true;
                }

                if (sell)
                {
                    sl = 0;
                    if (isIn)
                    {
                        Console.WriteLine($"{point.Date}: sell");
                        ss.Add(point.Date);
                    }
                    isIn = false;
                }

                //Console.WriteLine($"{point.Date}, {buyline}, {sellline}, {sl}");

            }

            return (bs, ss);

        }

        private double EMA(IEnumerable<double> series, double length, int offset = 0)
        {
            var ema = series.First();
            var k = 2.0 / (length + 1.0);
            foreach(var v in series.Take(series.Count() - offset))
            {
                ema = v * k + ema * (1 - k);
            }
            return ema;
        }

        private double ATR(IEnumerable<HistoryPrice> series, int length)
        {
            var ATR = 0.0;
            var pc = 0.0;

            var preCalc = 1;

            foreach(var point in series)
            {
                var v1 = point.High - point.Low;
                var v2 = pc == 0 ? v1 : Math.Abs(point.High - pc);
                var v3 = pc == 0 ? v1 : Math.Abs(point.Low - pc);
                //var TR = pc == 0 ? point.High - point.Low : Math.Max(point.High, pc) - Math.Min(point.Low, pc);
                var TR = Math.Max(Math.Max(v1, v2), v3);
                if (preCalc > 0)
                {
                    ATR += TR;
                }
                else
                {
                    ATR = (ATR * (length - 1) + TR) / length;
                }
                pc = point.Close;
                preCalc--;
                if(preCalc == 0)
                {
                    ATR = TR / 1;
                }
            }

            return ATR;
        }

    }

}
