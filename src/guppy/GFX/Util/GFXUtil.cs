using Guppy2.Calc;
using System;

namespace Guppy2.GFX
{


    

    public static class GFXUtil
    {

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

      /*  public static int RGBToBGR(int rgb)
        {
            int r = (rgb >> 16) & 0xff;
            int g = (rgb >> 8) & 0xff;
            int b = rgb & 0xff;
            return (b << 16) | (g << 8) | r;
        }*/

      /*  public static int BGRToRGB(int rgb)
        {
            //rgb->bgr actualy swaps red and blue component so we can use it here to
            return RGBToBGR(rgb);
        }*/


        /*public static bool Sort(ref int a,ref int b) {
            if(a>b) {
                int t=a;
                a=b;
                b=t;
                return true;
            }
            return false;
        }*/

        /*public static bool Sort(ref double a, ref double b)
        {
            if (a > b)
            {
                double t = a;
                a = b;
                b = t;
                return true;
            }
            return false;
        }*/

       
      

        /*public static void DecodeRGB(int value, out byte r, out byte g, out byte b)
        {
            r = (byte)((value & 0xff0000) >> 16);
            g = (byte)((value & 0xff00) >> 8);
            b = (byte)(value & 0xff);
        }*/

        /*public static int EncodeRGB(byte r, byte g, byte b)
        {
            return ((r << 16) + (g << 8) + b);
        }*/


       /* public static int ColorToGray(int rgb)
        {
            double r = ((rgb >> 16) & 255) / 255.0;
            double g = ((rgb >> 8) & 255) / 255.0;
            double b = (rgb & 255) / 255.0;
            int alpha = (rgb >> 24) & 255;    //keep alpha chanel as is

            int gray = RealToInt((0.3 * r + 0.59 * g + 0.11 * b)*255.0); //luma formula
            if (gray < 0) gray = 0;
            if (gray > 255) gray = 255;

            return (alpha<<24) | (gray << 16) | (gray << 8) | gray; //convert to grayscale rgb
        }*/


        /// <summary>
        /// inverts alpha channel so that it a becomes 255-a
        /// </summary>
        /*public static int InvertAlpha(int color)
        {
            int alpha = (255 - (color >> 24)) << 24;
            //int alpha = (255 - ((color & 0xff000000) >> 24)) << 24;
            return (color & 0xffffff) | alpha;
        }*/

        
        /*
        static Random rnd=new Random();
        public static int RandomColor
        {
            get
            {
                //if (rnd == null) rnd = new Random(42);
                int r = rnd.Next(256);
                int g = rnd.Next(256);
                int b = rnd.Next(256);
                return EncodeRGB((byte)r, (byte)g,(byte) b);
            }
        }*/

        /// <summary>
        /// Makes the line from x1,y1 to x2,y2 one unit longer by modifying x2,y2
        /// </summary>
        /*public static void ExtendByOne(double x1, double y1, ref double x2, ref double y2)
        {

            double dx = x2 - x1;
            double dy = y2 - y1;
            double len = dx * dx + dy * dy;

            if (len < 0.001) { x2++; return; } //zero length line, extend in whichever direction
            len = Math.Sqrt(len);

            x2 += dx / len;
            y2 += dy / len;
        }*/


        
    }
}
