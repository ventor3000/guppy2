using Guppy2.Calc.Geom2d;
using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2Test
{
    public delegate Point2d TrackingDelegate(Point2d pos,Painter painter);

    public class InputPoint:InputCore
    {

        public Point2d SelectedPoint = Point2d.Origo; //set to the selected point on success
        private TrackingDelegate trackingCallback;

        Point2d basePoint;

        public InputPoint(Point2d basept,TrackingDelegate trackingCallback)
        {
            this.basePoint = basept;
            this.trackingCallback = trackingCallback;
        }

        public override void OnMouseDown(Point2d p)
        {
            SelectedPoint = p;
            Exit();
        }


        public override void OnTracking(Point2i dcs, Point2d wcs, Painter painter)
        {
            painter.Color = RGB.White;
            painter.DrawMark(dcs.X, dcs.Y, MarkType.Cross, 50);

            if (trackingCallback != null)
            {
                Point2d trackpt = trackingCallback(wcs, painter);
            }
                    

            if (basePoint != null)
                painter.DrawLineT(basePoint.X, basePoint.Y, wcs.X, wcs.Y);
        }
    }
}
