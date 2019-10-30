using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Guppy2.AppUtils
{
    public static class ParseUtils
    {

        private static Regex regex_double = new Regex(@"^\s*[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?");
        private static Regex regex_ident = new Regex(@"^\s*([a-zA-Z_][a-zA-Z0-9_]*)");
        private static Regex regex_int = new Regex(@"^\s*([0-9]+)");


        public static bool ParseDouble(ref string str,out double d) {
            
            Match m = regex_double.Match(str, 0);

            if (m.Success) {
                if (double.TryParse(m.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) {
                    str = str.Substring(m.Length);
                    return true;
                }
            }

            d = 0.0;
            return false;
        }

        public static bool ParseInt(ref string str, out int i)
        {
            
            //we parse this as a double and the tries to convert it to integer, so that we dont
            //wrongly parses the first part of a double as an int
            Match m = regex_double.Match(str, 0);

            if (m.Success)
            {
                if (int.TryParse(m.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out i))
                {
                    str = str.Substring(m.Length);
                    return true;
                }
            }

            i = 0;
            return false;
        }


        
        public static bool ParseIdentifier(ref string str, out string ident)
        {
            ident = null;
            Match m = regex_ident.Match(str, 0);
            
            if (m.Success)
            {
                ident = m.Groups[1].Value;
                str = str.Substring(m.Length);
                return true;
            }

            return false;
        }

        public static void SkipWhite(ref string str)
        {
            int l = str.Length;
            int p = 0;
            while (p < l && char.IsWhiteSpace(str[p]))
                p++;

            if (p > 0)
                str = str.Substring(p);
        }
            
        
    }
}
