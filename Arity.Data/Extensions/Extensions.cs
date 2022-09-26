using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arity.Data.Extensions
{
    public static class Extensions
    {
        public static Tuple<DateTime?, DateTime?> GetDatesFromRange(this string range)
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;
            if (!string.IsNullOrWhiteSpace(range))
            {
                var dates = range.Split('-');
                if (!string.IsNullOrWhiteSpace(dates[0]))
                {
                    fromDate = DateTime.ParseExact(dates[0], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                if (!string.IsNullOrWhiteSpace(dates[1]))
                {
                    toDate = DateTime.ParseExact(dates[1], "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1).AddMilliseconds(-1);
                }
            }
            return Tuple.Create(fromDate, toDate);
        }
    }
}
