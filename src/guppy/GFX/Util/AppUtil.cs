using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Guppy2.GFX.Util
{
    /// <summary>
    /// Simple static functions that are general application programming helpers
    /// </summary>
    public class AppUtil
    {

        
        public static Random random = new Random();

        public static double RandDouble(double minval,double maxval) {return random.NextDouble()*(maxval-minval)+minval;}
        public static double RandDouble(double maxval) {return random.NextDouble()*maxval;}
        public static int RandInt(int minval, int maxval) { return random.Next(minval,maxval);}
        public static int RandInt(int maxval) { return random.Next(maxval); }


        static Stopwatch sw=new Stopwatch();
        public static long Time
        {
            get
            {
                long res = sw.ElapsedMilliseconds;
                sw.Reset();
                sw.Start();
                return res;
            }
        }


        public static void TimeReset()
        {
            sw.Reset();
            sw.Start();
        }
                    
                

    }
}
