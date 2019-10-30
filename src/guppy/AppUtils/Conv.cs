using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Guppy2.AppUtils
{

    public enum FloatFormat
        {
            General,
            Decimal,
            DecimalFixed,
            Exponent,
            ExponentFixed,           
            Exact
        }


    public static class Conv
    {


        static CultureInfo culture = CultureInfo.InvariantCulture;
        private static string numbers = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static char[] numbuf = new char[256]; //the longest buffer a real or int can ever be
        private const int default_decimals = 6;


        private static string Hashes(int num) { return new string('#', num); }
        private static string Zeroes(int num) { return new string('0', num); }
            

        
        public static string FloatToStr(double val, int decimals = 15 , FloatFormat form = FloatFormat.General)
        {
            switch (form)
            {
                case FloatFormat.Exponent:
                    return val.ToString("0."+Hashes(decimals)+"e0", culture);
                case FloatFormat.ExponentFixed:
                    return val.ToString("0." + Zeroes(decimals) + "e0", culture);
                case FloatFormat.Decimal:
                    return val.ToString("0." + Hashes(decimals), culture);
                case FloatFormat.DecimalFixed:
                    return val.ToString("0." + Zeroes(decimals), culture);
                case FloatFormat.Exact:
                    return val.ToString("r", culture);
                default:
                case FloatFormat.General:
                    culture.NumberFormat.NumberDecimalDigits = decimals;
                    return val.ToString(culture);
            }
        }

        
        public static string FloatToStr(double val)
        {
            return FloatToStr(val, default_decimals);
        }


        public static string FloatsToStr(IEnumerable<double> vals, int decimals=default_decimals,FloatFormat form=FloatFormat.General, string separator = ",")
        {
            List<string> res = new List<string>();
            foreach (double d in vals)
                res.Add(FloatToStr(d,decimals,form));

            return string.Join(", ", res);
        }

        public static string FloatsToStr(params double[] vals)
        {
            return FloatsToStr(vals, default_decimals);
        }


        public static string IntToStr(int val)
        {
            return val.ToString(culture);
        }

        public static int StrToInt(string str)
        {
            return int.Parse(str, culture);
        }


        /// <summary>
        /// Computes the length needed for buffer in IntToStr with radix
        /// </summary>
        /// <param name="num"></param>
        /// <param name="radix"></param>
        /// <returns></returns>
        private static int GetIntToStrBufferLen(int num, int radix)
        {
            int res = 0;
            if (num < 0)
            {
                res++;  //space for - sign
                num = -num;
            }
            do
            {
                num /= radix;
                res++;
            } while (num > 0);

            return res;
        }

        public static string IntToStr(int val, int radix)
        {

            if (radix < 2 || radix >= 36)
                throw new Exception("Invalid radix for IntToStr => must be 2>=radix<36");

            if (radix == 10)
                return IntToStr(val); //faster than this implementation

            int len = GetIntToStrBufferLen(val, radix);
            int index = len - 1;

            if (val < 0) //need - sign?
            {
                numbuf[0] = '-';
                val = -val;
            }

            do
            {
                int num = val % radix;
                numbuf[index--] = numbers[num];
                val /= radix;
            } while (val > 0);

            return new string(numbuf, 0, len);
        }

        public static int FloatToInt(double d)
        {
            if (d < 0.0)
                return (int)(d - 0.5);
            else
                return (int)(d + 0.5);
        }

        public static int FloatToInt(float f)
        {
            if (f < 0f)
                return (int)(f - 0.5f);
            else
                return (int)(f + 0.5f);
        }


    }
}
