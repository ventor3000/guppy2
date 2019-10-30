using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    /// <summary>
    /// Specialized class used as sizer for tab control.
    /// </summary>
    public class SizerTabs:SizerBase
    {
        private SizerTabs() { }

        public static SizerTabs Instance=new SizerTabs();


        public override void CalcChildrenFinalPos(Group group, Size2i size)
        {

            //final position for each tab is all the space it gets
            Rect2i area = GetChildLayoutArea(group, size);
            foreach (Widget w in group.Children)
                w.LayoutResult.FinalPos = area;
        }

        protected override Size2i CalcChildrenMinimumSize(Group group)
        {
            //the minimum size is the size of the largest child
            Size2i res = Size2i.Zero;
            foreach (Widget w in group.Children)
                res = Size2i.Max(res, w.LayoutResult.MinSize);
            return res;
        }

        public override void RecursiveCalcLayoutInfo(Group group)
        {
            throw new Exception("Should never be called"); //not used for this very simple sizer
        }
    }
}
