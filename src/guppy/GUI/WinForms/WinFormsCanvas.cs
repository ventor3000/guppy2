using Guppy2.GFX.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{
    public class WinFormsCanvas : WinFormsControl, IDriverCanvas
    {
        public WinFormsCanvas(GUIObject guiobject)
        {
            Init(new DrawPanel(), guiobject);
            Panel.BackColor = Color.White;

            Panel.Paint += Panel_Paint;
            Panel.MouseMove += Panel_MouseMove;
            Panel.MouseWheel += Panel_MouseWheel;
            Panel.Resize += Panel_Resize;
            Panel.MouseDown += Panel_MouseDown;
            Panel.MouseUp += Panel_MouseUp;
            
        }

        void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            Canvas c = Control.Tag as Canvas;
            c.TriggerButton(new ButtonEventArgs(false, e.Location.X, e.Location.Y, GetKeyStatus()));
        }

        void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            Canvas c = Control.Tag as Canvas;
            c.TriggerButton(new ButtonEventArgs(true, e.Location.X, e.Location.Y, GetKeyStatus()));
        }

        void Panel_Resize(object sender, EventArgs e)
        {
            Canvas c = Control.Tag as Canvas;
            c.TriggerResized(new EventArgs());
        }

        void Panel_MouseWheel(object sender, MouseEventArgs e)
        {
            Canvas c = Control.Tag as Canvas;
            c.TriggerWheel(new WheelEventArgs(e.Delta));
        }


        KeyStatus GetKeyStatus()
        {
            KeyStatus stat = KeyStatus.None;

            var mbut = Control.MouseButtons;

            if (mbut.HasFlag(MouseButtons.Left)) stat |= KeyStatus.LeftButton;
            if (mbut.HasFlag(MouseButtons.Right)) stat |= KeyStatus.RightButton;
            if (mbut.HasFlag(MouseButtons.Middle)) stat |= KeyStatus.MiddleButton;

            var keys = Control.ModifierKeys;
            if (keys.HasFlag(Keys.Control)) stat |= KeyStatus.Control;
            if (keys.HasFlag(Keys.Alt)) stat |= KeyStatus.Alt;
            if (keys.HasFlag(Keys.Shift)) stat |= KeyStatus.Shift;

            return stat;
        }

        void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            Canvas c = Control.Tag as Canvas;
            c.TriggerMotion(new MotionEventArgs(e.X,e.Y,GetKeyStatus()));
        }

        void Panel_Paint(object sender, PaintEventArgs e)
        {
            using (PainterGDIPlus pgdi = new PainterGDIPlus(Panel))
            {
                Canvas c = Control.Tag as Canvas;
                c.TriggerRedraw(new RedrawEventArgs(pgdi));
            }
        }

        System.Windows.Forms.Panel Panel
        {
            get { return Control as System.Windows.Forms.Panel; }
        }

        public GFX.Painter CreatePainter()
        {
            return new PainterGDIPlus(Panel);
        }

        public override Size2i NaturalSize
        {
            get { return new Size2i(100,100); }
        }



        private class DrawPanel : System.Windows.Forms.Panel
        {
            protected override void OnPaintBackground(PaintEventArgs e)
            {
                //do nothing to prevent background painting
            }


            //TODO: allow focus on click
        }

        
    }
}
