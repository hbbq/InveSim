using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InveSim.Simulator
{

    public class DateHandler
    {

        [JsonProperty("H")]
        public List<DateTime> Holidays = new List<DateTime>();

        public DateTime GetPreviousDate(DateTime date)
        {
            var d = date.AddDays(-1);
            while (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday || Holidays.Any(h => h == d))
            {
                d = d.AddDays(-1);
            }
            return d;
        }

        public DateTime GetNextDate(DateTime date)
        {
            var d = date.AddDays(1);
            while (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday || Holidays.Any(h => h == d))
            {
                d = d.AddDays(1);
            }
            return d;
        }

    }

}
