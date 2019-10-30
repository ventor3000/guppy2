using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{
    public abstract class WinFormsControl:IDriverWidget
    {
        public Control Control;


        ~WinFormsControl()
        {
            Dispose();
        }

        protected void Init(Control ctrl, GUIObject guiobj)
        {
            Control = ctrl;
            ctrl.Tag = guiobj; //so that we can map control to GUIObject backwards


            if (!(this is WinFormsWindow)) //windows has own handling of enter/leave
            {
                Control.Validating += (s, e) =>
                {
                    BlockableEventArgs be = new BlockableEventArgs();
                    Widget.TriggerLeave(be);
                    e.Cancel = be.Block;
                };

                Control.Enter += (s, e) => { Widget.TriggerEnter(new EventArgs()); };
            }
        }



        public abstract Size2i NaturalSize { get; }

     


        public void Dispose()
        {
            if(Control!=null)
                Control.Dispose();
        }

        virtual public string Caption
        {
            get
            {
                return Control.Text;
            }
            set
            {
                Control.Text = value ?? "";
            }
        }


        virtual public void Show()
        {
            Control.Show();
        }

        virtual public void Hide()
        {
            Control.Hide();
        }




        virtual public Size2i PhysicalSize
        {

            get
            {
                var s = Control.Size;
                return new Size2i(s.Width, s.Height);
            }
        }

        virtual public Rect2i Bounds
        {
            get
            {
                var b = Control.Bounds;
                return new Rect2i(b.X, b.Y, b.Width, b.Height);
            }

            set
            {
                Control.SetBounds(value.X, value.Y, value.Width, value.Height);
            }
        }


        virtual public Group Parent
        {
            get
            {
                if (Control.Parent == null) return null;
                Group res = Control.Parent.Tag as Group;
                return res;
            }
        }

        virtual public Widget Widget
        {
            get
            {
                return Control.Tag as Widget;
            }
        }

        virtual public object NativeObject { get { return Control; } }

        virtual public Typeface Typeface
        {
            set
            {
                var wf = value.NativeObject as System.Drawing.Font;
                if (wf != null)
                    Control.Font = wf;
            }
        }


        static ToolTip tooltip=null;
        virtual public string Tip
        {
            get
            {
                if (tooltip == null) return "";
                return tooltip.GetToolTip(Control) ?? "";
            }
            set
            {
                if (tooltip == null) tooltip = new ToolTip();
                tooltip.SetToolTip(Control, value);
            }
        }

        virtual public bool Focused
        {
            get
            {
                Form f = Control.FindForm();
                if (f != null)
                    return f.ActiveControl == Control;
                return Control.Focused;
            }
            set
            {
                Form f = Control.FindForm();

                if (value)
                {
                    if (f != null)
                        f.ActiveControl = Control;
                    Control.Focus();
                }
                else
                {
                    //try to turn of focus
                    if (f != null)
                    {
                        var ac = f.ActiveControl;
                        if (ac == Control)
                            f.ActiveControl = null;
                    }
                }
            }
        }

        virtual public bool Enabled
        {
            get { return Control.Enabled; }
            set { Control.Enabled = value; }
        }





        public void RedrawLater()
        {
            Control.Invalidate();
        }

        public void Redraw()
        {
            Control.Refresh();
        }
    }
}
