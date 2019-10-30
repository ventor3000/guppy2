using Guppy2.Calc.Geom2d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2Test
{
    public static class TC
    {


        public static Point2d GetPoint(string prompt, Point2d basept = null)
        {
            using (new Prompt(prompt))
            {
                InputPoint inpt = new InputPoint(basept,null);
                inpt.Run();
                return inpt.SelectedPoint;
            }
        }

        public static void AddEntity(Entity ent)
        {
            CurrentDrawing.DB.Add(ent);            
        }

        public static Drawing CurrentDrawing
        {
            get { return Program.MainForm.CurrentDrawing; }
        }
    }
}
