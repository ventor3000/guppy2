using System;
using System.Collections.Generic;
using Guppy2;
using System.Threading;
using System.Globalization;
using System.Diagnostics;
using Guppy2.GUI;
using Guppy2.GUI.ExtraWidgets;
using Guppy2.AppUtils;
using Guppy2.GFX;
using Guppy2.Calc.Geom2d;
using Guppy2.Calc;


//testfile
namespace Guppy2Test
{
	
    public static class Program
    {

        static Window w;
		static Button b1,b6,dropbtn;
        static Frame myframe;
        static Typeface tf;
        static Edit ed;
        

        public static Splitter split;

        static Transform2d viewtransform = Transform2d.Identity;
        static Point2i cursorpixel = Point2i.Origin;


        static Canvas canvas;

        static int mousex, mousey;


        public static WinMain MainForm = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Guppy.Open();
            /*Window win = new Window("Test window", WindowStyle.FrameMainWindow);
            
            canvas = new Canvas(win) {ExpandX=true,ExpandY=true};
            canvas.EvRedraw += canvas_EvRedraw;
            canvas.EvMotion += canvas_EvMotion;*/


            MainForm = new WinMain();

            Guppy.Run(MainForm);
        }

        static void canvas_EvMotion(object sender, MotionEventArgs e)
        {
            mousex = e.X;
            mousey = canvas.PhysicalSize.Height-e.Y-1;

            canvas.Redraw();
        }

        static void canvas_EvRedraw(object sender, RedrawEventArgs e)
        {
            var p = e.Painter;
            p.Clear(0);

            
            Ellipse2d el1 = new Ellipse2d(new Point2d(mousex, mousey), 200, 5, 1.0);
            Ellipse2d el2 = new Ellipse2d(new Point2d(400,500), 300, 5, -1);

            p.Color = RGB.Red;
            p.DrawEllipse(el1.Center.X, el1.Center.Y, el1.MajorRadius, el1.MinorRadius, el1.Rotation);
            p.Color = RGB.Green;
            p.DrawEllipse(el2.Center.X, el2.Center.Y, el2.MajorRadius, el2.MinorRadius, el2.Rotation);


            var intpts=Intersect2d.EllipseEllipse(el1, el2);

            
            if (intpts != null)
            {
                p.Color = RGB.Yellow;
                foreach (Point2d pt in intpts)
                {
                    p.DrawMark(pt.X, pt.Y, MarkType.DiagonalCross, 10);
                }
            }


            
            
        }
    }
}
