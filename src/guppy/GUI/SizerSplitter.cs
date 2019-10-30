using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class SizerSplitter:SizerBase
    {
        private SizerSplitter() { }

        public static SizerSplitter Instance = new SizerSplitter();


        public override void CalcChildrenFinalPos(Group group, Size2i size)
        {

            //ugly hack here: the size for each child panel is the size it already has, since 
            //the parent splitter is already laid out when here. Use this fact.


            Splitter splitter = group as Splitter;
            var rect1 = new Rect2i(Point2i.Origin, splitter.Panel1.DriverControl.PhysicalSize);
            splitter.Panel1.LayoutResult.FinalPos = rect1;
            var rect2 = new Rect2i(Point2i.Origin, splitter.Panel2.DriverControl.PhysicalSize);
            splitter.Panel2.LayoutResult.FinalPos = rect2;
        }

        protected override Size2i CalcChildrenMinimumSize(Group group)
        {
            Size2i res = Size2i.Zero;
            Splitter splitter = group as Splitter;
            int sw = splitter.SplitterWidth;

            if (splitter.Vertical)
            { //splitbar is vertical, sum widths
                res = new Size2i(
                    splitter.Panel1.LayoutResult.MinSize.Width + splitter.Panel2.LayoutResult.MinSize.Width + sw,
                    Math.Max(splitter.Panel1.LayoutResult.MinSize.Height, splitter.Panel2.LayoutResult.MinSize.Height)
                    );
            }
            else
            { //splitbar is horizontal, sum heights
                res = new Size2i(
                    Math.Max(splitter.Panel1.LayoutResult.MinSize.Width, splitter.Panel2.LayoutResult.MinSize.Width),
                    splitter.Panel1.LayoutResult.MinSize.Height + splitter.Panel2.LayoutResult.MinSize.Height + sw
                    );
            }

            return res;
        }
    }
}
