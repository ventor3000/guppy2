using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{

    [Flags]
    public enum KeyStatus
    {
        None=0,
        Shift=1,
        Control=2,
        Alt=4,
        LeftButton=8,
        MiddleButton=16,
        RightButton=32
    }


    public delegate void GUIEvent(object sender,EventArgs e);
    

    public class BlockableEventArgs : EventArgs
    {
        public bool Block = false;
    }
    public delegate void GUIEventBlockable(object sender, BlockableEventArgs e);


    public class KeyEventArgs : BlockableEventArgs
    {
        public KeyEventArgs(KeyData data)
        {
            this.KeyData = data;
        }
        public readonly KeyData KeyData;
    }
    public delegate void GUIEventKey(object sender, KeyEventArgs e);


    /*public class ResizedEventArgs : EventArgs
    {
        public readonly int Width,Height;

        public ResizedEventArgs(int w, int h)
        {
            this.Width=w;
            this.Height=h;
        }
    }
    public delegate void GUIEventResize(ResizedEventArgs e);*/


    public class MotionEventArgs : EventArgs
    {
        public readonly int X;
        public readonly int Y;
        public readonly KeyStatus Status;

        public MotionEventArgs(int x, int y,KeyStatus status)
        {
            this.X = x;
            this.Y = y;
            this.Status = status;
        }

        
    }
    public delegate void GUIEventMotion(object sender, MotionEventArgs e);


    public class ButtonEventArgs : EventArgs
    {
        public readonly int X;
        public readonly int Y;
        public readonly KeyStatus Status;
        public readonly bool Pressed;

        public ButtonEventArgs(bool pressed,int x, int y, KeyStatus status)
        {
            this.Pressed = pressed;
            this.X = x;
            this.Y = y;
            this.Status = status;
        }
    }
    public delegate void GUIEventButton(object sender, ButtonEventArgs e);


    public class RedrawEventArgs : EventArgs
    {
        public readonly Painter Painter;

        public RedrawEventArgs(Painter p)
        {
            Painter = p;
        }
    }
    public delegate void GUIEventRedraw(object sender, RedrawEventArgs e);


    public class WheelEventArgs : EventArgs
    {
        public readonly int Delta;

        public WheelEventArgs(int delta)
        {
            this.Delta = delta;
        }
    }
    public delegate void GUIEventWheel(object sender, WheelEventArgs e);


}
