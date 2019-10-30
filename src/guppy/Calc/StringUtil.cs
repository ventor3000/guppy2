using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc
{

    

    public static class StringUtil
    {
        static CultureInfo culture = CultureInfo.InvariantCulture;

        public static string FormatReal(double r,int decimals=15)
        {
            return Math.Round(r,decimals).ToString(culture);
            
        }

        public static string FormatReals(params double[] lst)
        {
            
            List<string> res = new List<string>();
            foreach (double d in lst)
                res.Add(FormatReal(d)); //TODO: exact formatting

            return string.Join(", ", res);
        }

        public static string FormatInt(int i)
        {
            return i.ToString(culture);
        }
        
    }
}
