using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using Guppy2.Calc;

namespace Guppy2.GFX.Windows
{
    public static class PenGenerator
    {
        /// <summary>
        /// Create a GDI+ Pen that corresponds to a Painter setup
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Pen CreateLinePenFromPainter(Painter p)
        {
            Color c=PainterGDIPlus.RGBToColor(p.Color);
            if (p.Opacity < 0.999)
                c = Color.FromArgb(GFXUtil.FloatToInt(MathUtil.Clamp(p.Opacity * 255.0, 0.0, 255.0)), c);
    
            Pen linepen = new Pen(c, (float)Math.Abs(p.LineWidth)); //0 means the thinnest possible on device


            switch (p.LineStyle)
            {
                case LineStyle.Continuous: break; //done already
                case LineStyle.Custom:
                    SetCustomDashes(p.LineStyleDashes, p.LineWidth, linepen);
                    linepen.DashStyle = DashStyle.Custom;
                    break;
                case LineStyle.Dash:
                    linepen.DashPattern = new float[] { 8, 8 };
                    linepen.DashStyle = DashStyle.Custom;
                    break;
                case LineStyle.DashDotDot:
                    linepen.DashPattern = new float[] { 7, 2, 2, 1, 2, 2 };
                    linepen.DashStyle = DashStyle.Custom;
                    break;
                case LineStyle.DashDot:
                    linepen.DashPattern = new float[] { 8, 3, 2, 3 };
                    linepen.DashStyle = DashStyle.Custom;
                    break;
                case LineStyle.Dot:
                    linepen.DashPattern = new float[] { 4, 4 };
                    linepen.DashStyle = DashStyle.Custom;
                    break;
            }


          /*  if (p.LineStyle != LineStyle.Continuous)
            {
                //linepen.DashStyle = LineStyleToDashStyle(p.LineStyle);
                linepen.DashStyle = DashStyle.Custom;
                if (p.LineStyle == LineStyle.Custom)
                    SetCustomDashes(p.LineStyleDashes, linepen);
                else if (p.LineStyle == LineStyle.Dot)
                    linepen.DashPattern = new float[] { 4, 4 };
                else
                    linepen.DashPattern = new float[] { 10, 4, 4, 4 };


                
            }
            */

            LineCap ecap;
            DashCap dcap;

            if (p.EndCaps != EndCap.Flat) //flat is default for pen and thus need no change
            {

                GetEndCap(p.EndCaps, out ecap, out dcap);
                linepen.EndCap = ecap;
                linepen.StartCap = ecap;
                linepen.DashCap = dcap;
            }


            if(p.LineJoin!=LineJoin.Miter)  //miter is default and thus needs no change
                linepen.LineJoin = GetLineJoin(p.LineJoin);

            return linepen;
        }

        
        /*
        private static DashStyle LineStyleToDashStyle(Turtle.LineStyle linestyle)
        {
            switch (linestyle)
            {
                case LineStyle.Continuous: return DashStyle.Solid;
                case LineStyle.Custom: return DashStyle.Custom;
                case LineStyle.Dash: return DashStyle.Dash;
                case LineStyle.DashDot: return DashStyle.DashDot;
                case LineStyle.DashDotDot: return DashStyle.DashDotDot;
                case LineStyle.Dot: return DashStyle.Dot;
                default: return DashStyle.Solid;


            }
        }
        */


        private static System.Drawing.Drawing2D.LineJoin GetLineJoin(Guppy2.GFX.LineJoin lj)
        {
            switch (lj)
            {
                case Guppy2.GFX.LineJoin.Bevel: return System.Drawing.Drawing2D.LineJoin.Bevel;
                case Guppy2.GFX.LineJoin.Miter: return System.Drawing.Drawing2D.LineJoin.Miter;
                default: return System.Drawing.Drawing2D.LineJoin.Round;
            }
        }


        private static void SetCustomDashes(double[] pat, double linewidth, Pen pen)
        {
            //always make even number of dash entries to fix fishy behaviour of gdi+
            
            float[] fpat;

            int n = pat.Length;
            float div = Math.Max(1f, (float)linewidth); //need to divide dashes to avoid GDI+ scaling with linewidth

            if ((n % 2) == 0)
                fpat = new float[n];
            else
                fpat = new float[n * 2];

            int m = fpat.Length;

            for (int l = 0; l < m; l++)
                fpat[l] = Math.Max((float)pat[l % n]/div, 0.1f);

            pen.DashPattern = fpat;
        }

        private static void GetEndCap(EndCap ec, out LineCap endcaps, out DashCap dashcaps)
        {

            switch (ec)
            {
                case EndCap.Flat: endcaps = LineCap.Flat; dashcaps = DashCap.Flat; break;
                case EndCap.Square: endcaps = LineCap.Square; dashcaps = DashCap.Flat; break;
                default: endcaps = LineCap.Round; dashcaps = DashCap.Round; break;
            }
        }
    }
}
