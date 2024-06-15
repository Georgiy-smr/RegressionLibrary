using Regression.Two_factor_regression.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regression.Two_factor_regression.Implements
{
    public class RowParser : IRowsParser
    {
        public IEnumerable<double> GetRowInput(string[] row,
            int countMembers,
            string targetName = "a")
        {
            var a = row.Where(x => !x.Equals("+"));
            var listAr = new double[countMembers];
            var sign = 1;
            foreach (var stringExpration in a)
            {
                if (stringExpration.Equals("-"))
                {
                    sign = -1;
                    continue;
                }

                if (!stringExpration.Contains("*a")) continue;
                var str = stringExpration.Remove(0, stringExpration!.IndexOf("a", StringComparison.Ordinal) + 1);
                var index = int.Parse(str);
                var st1r = stringExpration.Substring(0, stringExpration!.IndexOf("*", StringComparison.Ordinal));
                listAr[index] = sign * double.Parse(st1r, CultureInfo.InvariantCulture);
                sign = 1;
            }

            return listAr;
        }
        public IEnumerable<double> GetRowOutput(string[] column, int countMembers, string targetName = "a")
        {
            {
                var a = column.Where(x => !x.Equals("+"));
                var sign = 1;
                foreach (var stringExpration in a)
                {
                    if (stringExpration.Equals("-"))
                    {
                        sign = -1;
                        continue;
                    }
                    if (stringExpration.Contains("*a")) continue;
                    yield return sign * double.Parse(stringExpration, CultureInfo.InvariantCulture);
                    sign = 1;
                }
            }
        }
    }
}
