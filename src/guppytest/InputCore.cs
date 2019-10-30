using Guppy2;
using Guppy2.Calc.Geom2d;
using Guppy2.GFX;
using Guppy2.GUI.ExtraWidgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2Test
{
    public class InputCore
    {


        private bool running = false;

        public virtual void OnMouseDown(Point2d p)
        {

        }


        public virtual void OnTracking(Point2i dcs,Point2d wcs, Painter painter)
        {
            painter.Color = RGB.Red;
            painter.DrawCircle(dcs.X, dcs.Y, 20);
        }


        public virtual void Run()
        {

            running = true;

            var oldsink=Program.MainForm.CurrentSink;

            try
            {

                Program.MainForm.CurrentSink = this;

                while (running)
                {
                    Guppy.Wait(true);
                }

            }
            catch
            {
                throw;
            }
            finally
            {
                running = false;
                Program.MainForm.CurrentSink = oldsink;
            }
        }


        public virtual void Exit()
        {
            running = false; //to break out of Run() loop
        }

    }
}
