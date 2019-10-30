using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    /// <summary>
    /// Info for how a child widget should be laid out.
    /// </summary>
    public class LayoutInfo
    {
        public bool? ExpandX = null;
        public bool? ExpandY = null;
        public bool? UniformWidth = null;
        public bool? UniformHeight = null;
        public Align? Align = Guppy2.GUI.Align.TopLeft;
        public PackSide PackSide = PackSide.Top; //used in SizerPack
        public Size2i Size = Size2i.Natural;
    }
    
    
    public class Widget:GUIObject
    {
        public object Tag;
        
        private Group parent;

        internal LayoutResult LayoutResult; //computed by sizer in the layout process
        private LayoutInfo layout_info = new LayoutInfo();

        internal IDriverWidget DriverControl { get { return DriverObject as IDriverWidget; } }
        public Rect2i Bounds { get { return DriverControl==null ? null:DriverControl.Bounds; } set { if(DriverControl!=null) DriverControl.Bounds = value; } }
        

        public int Width {get {return Size.Width;} set {Size=new Size2i(value,Size.Height);}}
        public int Height { get { return Size.Height; } set { Size = new Size2i(Size.Width,value); } }


        #region LAYOUT_INFO_ACCESS
        public virtual LayoutInfo LayoutInfo{get {return layout_info;}}
        public virtual bool? ExpandX { get { return LayoutInfo.ExpandX; } set { LayoutInfo.ExpandX = value; } }
        public virtual bool? ExpandY { get { return LayoutInfo.ExpandY; } set { LayoutInfo.ExpandY = value; } }
        public virtual bool? UniformWidth { get { return LayoutInfo.UniformWidth; } set { LayoutInfo.UniformWidth = value; } }
        public virtual bool? UniformHeight { get { return LayoutInfo.UniformHeight; } set { LayoutInfo.UniformHeight = value; } }
        public virtual Align? Align { get { return LayoutInfo.Align; } set { LayoutInfo.Align = value; } }
        public virtual PackSide PackSide { get { return LayoutInfo.PackSide; } set { LayoutInfo.PackSide = value; } }
        public virtual Size2i Size { get { return LayoutInfo.Size; } set { LayoutInfo.Size = value; } }
        #endregion
        
        /// <summary>
        /// Returns the window that conatins this control
        /// </summary>
        public Window Window
        {
            get
            {
                if (this is Window)
                    return this as Window;
                if (Parent != null)
                    return Parent.Window;
                return null;
            }
        }

        public Group Parent
        {
            get
            {
                return parent;
            }
            internal set
            {
                parent = value;
            }
        }

        public virtual void Detach()
        {
            if(Parent!=null) 
                Parent.Children.Detach(this);
        }

        protected IDriverWidget DriverWidget {
            get {
                return DriverObject as IDriverWidget;
            }
        }

        public Typeface Typeface
        {
            set
            {
                var w = DriverWidget;
                if (w != null && value != null)
                    w.Typeface = value;
            }
        }

        public bool Focused
        {
            get
            {
                if (DriverWidget != null) return DriverWidget.Focused;
                return false;
            }
            set
            {
                if (DriverWidget != null) DriverWidget.Focused = value;
            }
        }

        public virtual bool Enabled
        {
            get
            {
                return DriverControl.Enabled;
            }
            set
            {
                DriverControl.Enabled = value;
            }
        }
            

        public virtual string Tip
        {
            get { if (DriverControl != null) return DriverControl.Tip; return ""; }
            set { if (DriverControl != null) DriverControl.Tip = value ?? ""; }
        }

        public virtual Size2i PhysicalSize
        {
            get
            {
                return DriverControl.PhysicalSize;
            }
        }

        public virtual void Redraw()
        {
            DriverControl.Redraw();
        }

        public virtual void RedrawLater()
        {
            DriverControl.RedrawLater();
        }

        public override void Dispose()
        {
            Detach();
            base.Dispose();
        }



        #region EVENTS

        public event GUIEvent EvEnter;
        protected virtual void OnEnter(EventArgs e) { if (EvEnter != null) EvEnter(this,e); }
        internal void TriggerEnter(EventArgs e) { OnEnter(e); }

        
        public event GUIEventBlockable EvLeave;
        protected virtual void OnLeave(BlockableEventArgs e) { if (EvLeave != null) EvLeave(this,e); }
        internal void TriggerLeave(BlockableEventArgs e) { OnLeave(e); }

        #endregion




    }
}
