using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class SizerPack:SizerBase
    {
        public static readonly SizerPack Instance = new SizerPack();

        public override void CalcChildrenFinalPos(Group group, Size2i size)
        {

            size = Size2i.Max(size, group.LayoutResult.MinSize);

            //Compute the space we have to distribute among expanding controls:
            int expandspacex = Math.Max(0, size.Width - group.LayoutResult.MinSize.Width);
            int expandspacey = Math.Max(0, size.Height - group.LayoutResult.MinSize.Height);

            //Find the internal rectangle to layout inside
            Rect2i area = GetChildLayoutArea(group, size);


            //compute number of controls that expands in some direction
            int numxexpanding = 0;
            int numyexpanding = 0;
            foreach(Widget child in group.Children) {
                if (child.LayoutResult.ExpandX) numxexpanding++;
                if (child.LayoutResult.ExpandY) numyexpanding++;
            }


            int ci = group.Children.Count;
            foreach(Widget child in group.Children) {
                PackSide chpack = child.PackSide;

                Rect2i alloc;

                bool last = (ci == group.Children.Count - 1); //last control laid out?
                int gapx = (ci == group.Children.Count - 1) ? 0:group.GapX; //no gap added for last control
                int gapy = (ci == group.Children.Count - 1) ? 0 : group.GapY; //no gap added for last control

                int extrax = 0, extray = 0;
                if (child.LayoutResult.ExpandX)
                {
                    if (last) //last control can expand on all remaining space
                        extrax = Math.Max(0, area.Width - child.LayoutResult.MinSize.Width);
                    else
                        extrax = DistributSpace(ci, numxexpanding, expandspacex);
                }

                if (child.LayoutResult.ExpandY)
                {
                    if(last) //last control can expand on all remaining space
                        extray=Math.Max(0,area.Height-child.LayoutResult.MinSize.Height);
                    else
                        extray = DistributSpace(ci, numyexpanding, expandspacey);
                }

                if ((chpack & PackSide.Bottom) != 0)
                {
                    alloc = new Rect2i(area.Left, area.Bottom-child.LayoutResult.MinSize.Height-extray, area.Width, child.LayoutResult.MinSize.Height+extray);
                    area = area.Shrink(new Margin(0, 0,0,alloc.Height + gapy));
                }
                else if ((chpack & PackSide.Left) != 0)
                {
                    alloc = new Rect2i(area.Left, area.Top,child.LayoutResult.MinSize.Width+extrax,area.Height);
                    area = area.Shrink(new Margin(alloc.Width+gapx, 0, 0, 0));
                }
                else if ((chpack & PackSide.Right) != 0)
                {
                    alloc = new Rect2i(area.Right-child.LayoutResult.MinSize.Width-extrax,area.Top,child.LayoutResult.MinSize.Width+extrax,area.Height);
                    area = area.Shrink(new Margin(0, 0, alloc.Width+gapx,0));
                }
                else //PackTop or packing not set which defaults to top
                {
                    alloc = new Rect2i(area.Left, area.Top, area.Width, child.LayoutResult.MinSize.Height+extray);
                    area = area.Shrink(new Margin(0, alloc.Height+gapy, 0, 0));
                }
                
                
               // child.LayoutResult.FinalPos = StickRectangle(alloc, child.LayoutResult.MinSize, FinalStick(child));

            }
        }

        protected override Size2i CalcChildrenMinimumSize(Group group)
        {
            int n=group.Children.Count;
            if (n <= 0)
                return Size2i.Zero;
            int w = 0;
            int h = 0;
            bool addgap=false; //we dont add gap for the last child, which is the first we compute size for


            foreach(Widget child in group.Children) {
                Size2i sz = child.LayoutResult.MinSize;
                
                if ((child.PackSide & PackSide.Top) != 0 || (child.PackSide & PackSide.Bottom) != 0)
                {
                    w = Math.Max(w, sz.Width);
                    h += sz.Height;
                    if (addgap) h += group.GapY;
                }
                else  //left or right packing, or assume top on none set
                {
                    w += sz.Width;
                    h = Math.Max(h,sz.Height);
                    if (addgap) w += group.GapX;
                }

                addgap = true;
            }

            return new Size2i(w, h);


           
        }
    }
}
