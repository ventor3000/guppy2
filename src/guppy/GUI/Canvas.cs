using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class Canvas:Widget
    {
        public Canvas(Group parent)
        {
            Construct(Guppy.Driver.CreateCanvas(this));
            if (parent != null)
                parent.Append(this);
        }


        IDriverCanvas DriverCanvas
        {
            get
            {
                return (IDriverCanvas)DriverObject;
            }
        }

        public Painter CreatePainter()
        {
            return DriverCanvas.CreatePainter();
        }

        #region EVENTS

        public event GUIEventMotion EvMotion;
        protected virtual void OnMotion(MotionEventArgs e) { if (EvMotion != null) EvMotion(this,e); }
        internal void TriggerMotion(MotionEventArgs e) { OnMotion(e); }

        public event GUIEventRedraw EvRedraw;
        protected virtual void OnRedraw(RedrawEventArgs e) { if (EvRedraw != null) EvRedraw(this,e); }
        internal void TriggerRedraw(RedrawEventArgs e) { OnRedraw(e); }

        public event GUIEventWheel EvWheel;
        protected virtual void OnWheel(WheelEventArgs e) { if (EvWheel != null) EvWheel(this, e); }
        internal void TriggerWheel(WheelEventArgs e) { OnWheel(e); }

        public event GUIEvent EvResized;
        protected virtual void OnResized(EventArgs e) {if (EvResized != null) EvResized(this, e);}
        internal void TriggerResized(EventArgs e) { OnResized(e); }

        public event GUIEventButton EvButton;
        protected virtual void OnButton(ButtonEventArgs e) { if (EvButton != null) EvButton(this, e); }
        internal void TriggerButton(ButtonEventArgs e) { OnButton(e); }

        #endregion


        
    }
}
