using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{

    public class GroupLayoutInfo
    {
        public bool ChildrenExpandX = false;
        public bool ChildrenExpandY = false;
        public bool ChildrenUniformWidth = false;
        public bool ChildrenUniformHeight = false;
        public Align ChildrenAlign = Align.TopLeft;
        public Margin Margin = Margin.None;
        public int GapX = Guppy.DefaultGap;
        public int GapY = Guppy.DefaultGap;
        public bool Vertical = true; //layout direction (in table)
        public int WrapCount = 1;   //number of controls per line (in table)
        public SizerBase Sizer = SizerTable.Instance;
    }

    public abstract class Group : Widget
    {
        internal readonly WidgetCollection Children;


        internal GroupLayoutInfo group_layout_info = new GroupLayoutInfo();

        #region GROUP_LAYOUT_INFO_ACCESS
        public virtual GroupLayoutInfo GroupLayoutInfo {get {return group_layout_info;}}
        public virtual bool ChildrenExpandX { get { return GroupLayoutInfo.ChildrenExpandX; } set { GroupLayoutInfo.ChildrenExpandX = value; } }
        public virtual bool ChildrenExpandY { get { return GroupLayoutInfo.ChildrenExpandY; } set { GroupLayoutInfo.ChildrenExpandY = value; } }
        public virtual bool ChildrenUniformWidth { get { return GroupLayoutInfo.ChildrenUniformWidth; } set { GroupLayoutInfo.ChildrenUniformWidth = value; } }
        public virtual bool ChildrenUniformHeight { get { return GroupLayoutInfo.ChildrenUniformHeight; } set { GroupLayoutInfo.ChildrenUniformHeight = value; } }
        public virtual Align ChildrenAlign { get { return GroupLayoutInfo.ChildrenAlign; } set { GroupLayoutInfo.ChildrenAlign = value; } }
        public virtual Margin Margin { get { return GroupLayoutInfo.Margin; } set { GroupLayoutInfo.Margin = value; } }
        public virtual int GapX { get { return GroupLayoutInfo.GapX; } set { GroupLayoutInfo.GapX = value; } }
        public virtual int GapY { get { return GroupLayoutInfo.GapY; } set { GroupLayoutInfo.GapY = value; } }
        public virtual bool Vertical { get { return GroupLayoutInfo.Vertical; } set { GroupLayoutInfo.Vertical = value; } }
        public virtual int WrapCount { get { return GroupLayoutInfo.WrapCount; } set { GroupLayoutInfo.WrapCount = value; } }
        public virtual Sizer Sizer
        {
            set
            {
                switch (value)
                {
                    case Sizer.Table: group_layout_info.Sizer = SizerTable.Instance; break;
                    case Sizer.Pack: group_layout_info.Sizer = SizerPack.Instance; break;
                    default: throw new Exception("Unimplemented sizer: " + value.ToString());
                }
            }
        }
        #endregion


        internal virtual IDriverGroup DriverGroup { get { return DriverObject as IDriverGroup; } }

        public virtual void Append(Widget ctrl) {Children.Add(ctrl);}

        virtual public Margin BorderDecorMargin { get { return DriverGroup.BorderDecorMargin; } }

        public virtual SizerBase SizerObject { get { return GroupLayoutInfo.Sizer; }  }

        public Group()
        {
            Children = new WidgetCollection(this);
        }

        /// <summary>
        /// Shortcut to set GapX and GapY at the same time
        /// </summary>
        public int Gap
        {
            set
            {
                GapX = value;
                GapY = value;
            }
            get
            {
                return GapX;
            }
        }

        virtual public void RefreshChildren()
        {
            SizerObject.RecursiveCalcLayoutInfo(this);
            SizerObject.RecursiveCalcFinalPos(this, Size2i.Max(Bounds.Size, LayoutResult.MinSize));
           // RecursiveLayoutChildren();
        }

        virtual public void Refresh()
        {
            RefreshChildren();
            var oldbounds = Bounds;
            Rect2i newbounds = new Rect2i(oldbounds.X, oldbounds.Y, LayoutResult.MinSize.Width, LayoutResult.MinSize.Height);
            Bounds = newbounds;
        }

      

        public Widget this[int index] {
            get {
                return Children[index];
            }
        }

        public override void Dispose()
        {
            //disposing a group disposes all its children as well
            while (Children.Count > 0)
                Children[0].Dispose();
            base.Dispose();
        }



    }
}
