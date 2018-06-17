using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieBarCodeGenerator.Core
{
    public static class Utils
    {
        /// <summary>
        /// Always return a floating point notation formatted number, with a leading zero, and up to 15 decimals of precision.
        /// The invariant culture is used ('.' as a decimal separator, no thousands separator...)
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ToInvariantString(this double number) => number.ToString("0.###############", CultureInfo.InvariantCulture);
    }
}
