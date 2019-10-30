using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{


    public enum Sizer
    {
        Table,
        Pack,
    }

    
    public class LayoutResult
    {
        //Computed during stage 1 of layout
        public Size2i MinSize;      //the minimum size this control can have, taking uniform width/height into account. (but not expand)
        public bool ExpandX=false;  //if this or any child control has x expand
        public bool ExpandY=false;  //if this or any child control has y expand

        //Computed during stage 2 of layout
        public Rect2i FinalPos;
    }

    
    public abstract class SizerBase
    {

        /// <summary>
        /// Recursivly computes the LayoutInfo for this control and all children.
        /// Must be called before RecursiveLayout
        /// </summary>
        /// <param name="group"></param>
        public virtual void RecursiveCalcLayoutInfo(Group group)
        {
            bool xpx = false, xpy = false;
            ComputeGroupLayoutInfo(group, ref xpx, ref xpy);
        }

        /// <summary>
        /// Does the physical layout of all children. Call RecursiveCalcLayoutInfo first!
        /// </summary>
        /// <param name="group">The group to layout.</param>
        /// <param name="area">The area to fit the child controls to.</param>
        public abstract void CalcChildrenFinalPos(Group group,Size2i size);

        public void RecursiveCalcFinalPos(Group group,Size2i size)
        {
            // Compute the LayoutResult.FinalPos for all children

            group.group_layout_info.Sizer.CalcChildrenFinalPos(group, size);
            
            Point2i org=group.DriverGroup.Origin;

            foreach (Widget w in group.Children)
            {
                //position each child before laying out is child controls, since this might be needed
                //for the layout logic of the childs (for example splitter, needs to access size of splitter panels)
                if (w.DriverObject != null && group.DriverGroup != null)
                {
                    Rect2i bounds = w.LayoutResult.FinalPos;
                    if (org != Point2i.Origin)
                       //the normal case org is Origin, but forexample winforms frame need another origin
                        bounds=bounds.Translate(org.X, org.Y);
                    
                    w.DriverControl.Bounds = bounds;
                }
                    

                //and now compute final pos for each child
                Group subgrp = w as Group;
                if (subgrp != null)
                    RecursiveCalcFinalPos(subgrp,subgrp.LayoutResult.FinalPos.Size);
            }

            
        }
        
        /// <summary>
        /// Computes space distribution, given an index and a total number of controls
        /// and a total space. This is needed instead of division to avoid roundoff 
        /// problems with many controls.
        /// </summary>
        /// <returns>The space distributed to the control with index</returns>
        protected int DistributSpace(int index, int outof, int space)
        {
            if (outof == 0) return 0;
            int rounddown = (int)(space / outof);
            int chompindex = space - rounddown*outof;

            if (index < chompindex)
                return rounddown + 1;
            else
                return rounddown;
        }

        /// <summary>
        /// Given a group and an outer size of it, calculates the rectangle inside where the controls should be laid out.
        /// This is done by subtracting the border decor and margin from it and placing the rectangle at the client of
        /// the group. 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        protected Rect2i GetChildLayoutArea(Group group, Size2i size)
        {
            var marg=group.Margin;

            size = size.Shrink(group.BorderDecorMargin);
            size = size.Shrink(marg);

            Rect2i area=new Rect2i(new Point2i(marg.Left,marg.Top), size);

            return area;
        }

      

       


        protected Rect2i AlignRectangle(Rect2i alloc, Size2i size, Align alg,bool expandx,bool expandy)
        {
            int x, y;
            int w = size.Width, h = size.Height;

            if (expandx)
            {
                x = alloc.Left;
                w = alloc.Width;
            }
            else if ((alg & Align.Right) != 0)
                x = alloc.Right - size.Width;
            else if ((alg & Align.Left) != 0)
                x = alloc.Left;
            else //centerd in x
                x = alloc.Left + alloc.Width / 2 - size.Width / 2;

            if (expandy)
            {
                y = alloc.Top;
                h = alloc.Height;
            }
            else if ((alg & Align.Top) != 0)
                y = alloc.Top;
            else if ((alg & Align.Bottom) != 0)
                y = alloc.Bottom - size.Height;
            else
                y = alloc.Top + alloc.Height / 2 - size.Height / 2;

            return new Rect2i(x, y, w,h);
        }


        /// <summary>
        /// Computes stage 1 layout info for a control that is not a group,
        /// which means setting the ExpandX,ExpandY and MinSize of the LayoutInfo of the control.
        /// </summary>
        /// <param name="control">The control to create a new LayoutInfo for</param>
        /// <param name="recexpandx">Changed to true if this control expands horizontally</param>
        /// <param name="recexpandy">Changed to true if this control expands vertically</param>
        private void ComputeControlLayoutInfo(Widget control, ref bool recexpandx, ref bool recexpandy)
        {
            control.LayoutResult = new LayoutResult();

            control.LayoutResult.ExpandX = FinalExpandX(control);
            control.LayoutResult.ExpandY = FinalExpandY(control);

            if (control.LayoutResult.ExpandX) recexpandx = true;
            if (control.LayoutResult.ExpandY) recexpandy = true;

            Size2i natsize = Size2i.Zero;
            int w, h;
            if (control.Size.Width < 0 || control.Size.Height < 0)
            { //width or height uses natural size
                if (control.DriverControl != null)
                    natsize = control.DriverControl.NaturalSize;
            }
            if (control.Size.Width < 0) w = natsize.Width; else w = control.Size.Width;
            if (control.Size.Height < 0) h = natsize.Height; else h = control.Size.Height;

            control.LayoutResult.MinSize = new Size2i(w, h);
        }


        /// <summary>
        /// Computes the MinSize field for a group, assuming stage 1 layout info is 
        /// calculated for all its children. Must be overridden by a sizer.
        /// </summary>
        /// <param name="group">The group to compute for.</param>
        protected abstract Size2i CalcChildrenMinimumSize(Group group);


        /// <summary>
        /// Computes stage 1 layout info for a group, recursivly computing it for all children inside.
        /// </summary>
        /// <param name="group">The group to compute for.</param>
        /// <param name="recexpandx">Set to true if this group should expand in x direction</param>
        /// <param name="recexpandy">Set to true if this group should expand int y direction</param>
        private void ComputeGroupLayoutInfo(Group group, ref bool recexpandx, ref bool recexpandy)
        {
            bool childexpandx = false, childexpandy = false;

            //compute layout info for all children
            foreach(Widget ctrl in group.Children) {
                if (ctrl is Group)
                    ((Group)ctrl).SizerObject.ComputeGroupLayoutInfo((Group)ctrl, ref childexpandx, ref childexpandy);
                else
                    ComputeControlLayoutInfo(ctrl, ref childexpandx, ref childexpandy);
            }


            group.LayoutResult = new LayoutResult();

            //If the expand for a group is not set, it expands if any of it children expands
            //otherwise it uses its expand setting for the layout
            if (group.ExpandX == null) group.LayoutResult.ExpandX = childexpandx;
            else group.LayoutResult.ExpandX = (bool)group.ExpandX;
            if (group.ExpandY == null) group.LayoutResult.ExpandY = childexpandy;
            else group.LayoutResult.ExpandY = (bool)group.ExpandY;

            //Resolve uniform width and height, which modifies all children
            //that is uniform to have the size of the largest uniform child
            int unih = 0, uniw = 0;
            List<Widget> modifyuniform = new List<Widget>();
            foreach(Widget ctrl in group.Children) {
                bool mod = false;
                if (FinalUniformHeight(ctrl)) { unih = Math.Max(unih, ctrl.LayoutResult.MinSize.Height); mod = true; }
                if (FinalUniformWidth(ctrl)) { uniw = Math.Max(uniw, ctrl.LayoutResult.MinSize.Width); mod = true; }
                if (mod) modifyuniform.Add(ctrl);
            }
            foreach (Widget ctrl in modifyuniform) //modify all children with uniform properties
            {
                int w = ctrl.LayoutResult.MinSize.Width;
                int h = ctrl.LayoutResult.MinSize.Height;
                if (FinalUniformWidth(ctrl)) { w = uniw; }
                if (FinalUniformHeight(ctrl)) { h = unih; }
                ctrl.LayoutResult.MinSize = new Size2i(w, h);
            }


            //finally we can compute the size for this group, taking the minimum size of the children
            //and append the margin and border decor of the group
            group.LayoutResult.MinSize = group.SizerObject.CalcChildrenMinimumSize(group);
            group.LayoutResult.MinSize = group.LayoutResult.MinSize.Grow(group.BorderDecorMargin);
            group.LayoutResult.MinSize = group.LayoutResult.MinSize.Grow(group.Margin);
            

            //flag any childs expanding to parent caller
            if (group.LayoutResult.ExpandX) recexpandx = true;
            if (group.LayoutResult.ExpandY) recexpandy = true;
        }

        /// <summary>
        /// Computes if a control should expand in x dir., using its own setting if set, otherwise consults
        /// parents ChildrenExpandX
        /// </summary>
        protected bool FinalExpandX(Widget ctrl)
        {
            if (ctrl.ExpandX == null)
            {
                if (ctrl.Parent == null) return false;
                return ctrl.Parent.ChildrenExpandX;
            }
            return (bool)ctrl.ExpandX;
        }

        /// <summary>
        /// Computes if a control should expand in y dir., using its own setting if set, otherwise consults
        /// parents ChildrenExpandY
        /// </summary>
        protected bool FinalExpandY(Widget ctrl)
        {
            if (ctrl.ExpandY == null)
            {
                if (ctrl.Parent == null) return false;
                return ctrl.Parent.ChildrenExpandY;
            }
            return (bool)ctrl.ExpandY;
        }

        /// <summary>
        /// Computes if a control should have uniform width, using its own setting if set, otherwise consults
        /// parents ChildrenUniformWidth
        /// </summary>
        protected bool FinalUniformWidth(Widget ctrl)
        {
            if (ctrl.UniformWidth == null)
            {
                if (ctrl.Parent == null) return false;
                return ctrl.Parent.ChildrenUniformWidth;
            }
            return (bool)ctrl.UniformWidth;
        }

        /// <summary>
        /// Computes if a control should have uniform height, using its own setting if set, otherwise consults
        /// parents ChildrenUniformHeight
        /// </summary>
        protected bool FinalUniformHeight(Widget ctrl)
        {
            if (ctrl.UniformHeight == null)
            {
                if (ctrl.Parent == null) return false;
                return ctrl.Parent.ChildrenUniformHeight;
            }
            return (bool)ctrl.UniformHeight;
        }

       
        protected Align FinalAlign(Widget ctrl)
        {
            if (ctrl.Align == null)
            {
                if (ctrl.Parent == null) return Align.TopLeft;
                return ctrl.Parent.ChildrenAlign;
            }
            return (Align)ctrl.Align;
        }
    }
}
