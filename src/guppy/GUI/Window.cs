using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{

    public enum WindowSize
    {
        Minimal,
        Current,
    }

    public enum WindowPosition
    {
        Auto,
        Current,
        CenterScreen,
        CenterParent
    }

    public enum WindowStyle
    {
        FrameDialog = 0,
        FrameMainWindow = Resizable|MinButton|MaxButton|TaskBarButton|SystemMenu,
        
        Resizable = 1,
        MinButton = 2,
        MaxButton = 4,
        TaskBarButton = 8,
        SystemMenu = 16,
    }
    
    public class Window:Group
    {
        bool shrink=false;

        protected IDriverWindow DriverWindow;

        private WindowPosition initial_position = WindowPosition.CenterParent;
        private WindowSize initial_size = WindowSize.Minimal;
        private string response = string.Empty;

        
        public Window(string caption,WindowStyle style)
        {
            DriverWindow=Guppy.Driver.CreateWindow(this, caption,style);
            Construct(DriverWindow);
        }
        
        public virtual void Show() 
        {
            response = "";            
            DriverWindow.Show();
        }

        public virtual string Response
        {
            get
            {
                return response;
            }

            set
            {
                Close(response);
            }
        }
        
        public virtual void Close(string response)
        {
            this.response = response ?? "";
            DriverWindow.Close();
        }

        public virtual string Popup()
        {
            response = string.Empty;
            DriverWindow.ShowModal();
            return response;
        }

        public virtual string Caption { get { return DriverWindow.Caption; } set { DriverWindow.Caption = value; } }

       

        public override void RefreshChildren()
        {
            base.RefreshChildren();

            if (!shrink)
                DriverWindow.MinSize = LayoutResult.MinSize;
            else
                DriverWindow.MinSize = Size2i.Zero;
        }

        public virtual bool Shrink
        {
            get { return shrink; }
            set { shrink = value; }
        }

        public virtual Button AcceptButton
        {
            get
            {
                return DriverWindow.AcceptButton;
            }
            set {
                DriverWindow.AcceptButton = value; 
            }
        }

        public virtual WindowSize InitialSize
        {
            get { return initial_size; }
            set { initial_size = value; }
        }

        public virtual WindowPosition InitialPosition
        {
            get { return initial_position; }
            set { initial_position = value; }
        }

        public virtual bool AutoDispose
        {
            get { return DriverWindow.AutoDispose; }
            set { DriverWindow.AutoDispose = value; }
        }


        public Widget ActiveChild
        {
            get
            {
                return DriverWindow.ActiveChild;
                
            }
        }


        #region EVENTS

        public event GUIEvent EvResized;
        protected virtual void OnResized(EventArgs e) {
            RefreshChildren(); 
            if (EvResized != null) EvResized(this,e);
        }
        internal void TriggerResized(EventArgs e) { OnResized(e); }

        public event GUIEvent EvShowing;
        protected virtual void OnShowing(EventArgs e) {if (EvShowing != null) EvShowing(this,e);}
        internal void TriggerShowing(EventArgs e) { OnShowing(e); }

        public event GUIEvent EvShown;
        protected virtual void OnShown(EventArgs e) { if (EvShown != null) EvShown(this,e); }
        internal void TriggerShown(EventArgs e) { OnShown(e); }

        public event GUIEvent EvClosed;
        protected virtual void OnClosed(EventArgs e) { if (EvClosed != null) EvClosed(this,e); }
        internal void TriggerClosed(EventArgs e) { OnClosed(e); }

        public event GUIEventBlockable EvClosing;
        protected virtual void OnClosing(BlockableEventArgs e) { if (EvClosing != null) EvClosing(this,e); }
        internal void TriggerClosing(BlockableEventArgs e) { OnClosing(e); }

        public event GUIEventKey EvKeyPreview;
        protected virtual void OnKeyPreview(KeyEventArgs e) { if (EvKeyPreview != null) EvKeyPreview(this,e); }
        internal void TriggerKeyPreview(KeyEventArgs e) { OnKeyPreview(e); }


        #endregion

    }
}
