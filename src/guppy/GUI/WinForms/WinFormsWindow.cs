using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using Guppy2.GUI;
using Guppy2.GFX;

namespace Guppy2.GUI.WinForms
{
    [DesignerCategory("")]
    public class WinFormsWindow : WinFormsGroupControl, IDriverWindow
    {
        private bool autodispose = true;

        public WinFormsWindow(GUIObject guiobject, string caption,WindowStyle style)
        {
            Init(new CustomForm(guiobject as Window), guiobject);
            Caption = caption;
            
            Form.StartPosition = FormStartPosition.Manual;  //we do the placement
            
            SetStyle(style);

            Form.Resize += (s, e) => { ((Window)guiobject).TriggerResized(new EventArgs());};
            Form.Shown += (s, e) => { ((Window)guiobject).TriggerShown(new EventArgs()); };
            Form.Load += (s, e) => { ((Window)guiobject).TriggerShowing(new EventArgs()); };
            Form.FormClosing += Form_FormClosing; 
            Form.FormClosed += (s, e) => { ((Window)guiobject).TriggerClosed(new EventArgs()); };

            //Note: enter/validate are not triggered in form, we duplicate it here:
            Form.Activated += (s, e) => { ((Window)guiobject).TriggerEnter(new EventArgs()); };
            Form.Deactivate += (s, e) => {
                //Note: deactivating a window is a blockable event but cannot be blocked in reality
                ((Window)guiobject).TriggerLeave(new BlockableEventArgs());
            };
            

            //Note: KeyPreview is triggered in inherited form due to private access
            
        }

        private void SetStyle(WindowStyle style)
        {
            Form.MaximizeBox = (style & WindowStyle.MaxButton) != 0;
            Form.MinimizeBox = (style & WindowStyle.MinButton) != 0;
            Form.FormBorderStyle = (style & WindowStyle.Resizable) != 0 ? FormBorderStyle.Sizable : FormBorderStyle.FixedDialog;
            Form.ShowIcon = (style & WindowStyle.SystemMenu) != 0 ? true : false;
            Form.ShowInTaskbar = (style & WindowStyle.TaskBarButton) != 0 ? true : false;
        }

       

        void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Window window = ((Window)Control.Tag);

            BlockableEventArgs edata = new BlockableEventArgs();
            ((Window)Control.Tag).TriggerClosing(edata);

            if (edata.Block)
            {
                e.Cancel = true;
                return;
            }
            
            //use default behaviour for modal winforms
            if (Form.Modal)
                return;

            //do not block mainwindow from closing if we for some stupid reason has set AutoDispose to false for it
            if (window == Guppy.MainWindow)
                return;
            
            //avoid normal close which will dispose forms which are not shown modally
            if (AutoDispose)
                e.Cancel = false;
            else
            {
                //check if the mainform is still open

                if (e.CloseReason == CloseReason.ApplicationExitCall || e.CloseReason == CloseReason.WindowsShutDown)
                    return; //allow close on application exit

                e.Cancel = true;
                Form.Hide();
            }
        }
        
        public bool AutoDispose
        {
            get { return autodispose; }
            set { autodispose = value; }
        }

        public Form Form { get { return Control as Form; } }


        public Size2i MinSize
        {
            get
            {
                var s = Form.MinimumSize;
                return new Size2i(s.Width, s.Height);
            }
            set
            {
                Form.MinimumSize = new Size(value.Width, value.Height);
            }
        }


        public void ShowModal()
        {
            UpdateWindowPlacement();
            Form.ShowDialog();
        }

        public override void Show()
        {
            UpdateWindowPlacement();
            Form.Show();
        }

        public void Close()
        {
            if(Form.Visible) //stupid winforms disposes form but do not call FormClosing event if not visible, we need this so hack around it
                Form.Close();
        }

        public Margin BorderDecorMargin
        {
            get
            {
                int xsiz, ysiz;


                xsiz = (Control.Width - Control.ClientSize.Width)/2;
                ysiz = xsiz;

                /*
                This code does not work in windows8 + debug because of bug(?) in FrameBorderSize property 
                if (Form.FormBorderStyle == FormBorderStyle.Sizable)
                {
                    xsiz = SystemInformation.FrameBorderSize.Width; // HorizontalResizeBorderThickness;
                    ysiz = SystemInformation.FrameBorderSize.Height; // VerticalResizeBorderThickness;
                }
                else
                {
                    xsiz = SystemInformation.FixedFrameBorderSize.Width;
                    ysiz = SystemInformation.FixedFrameBorderSize.Height;
                }*/

                return new Margin(
                  xsiz,
                  SystemInformation.CaptionHeight + ysiz,
                  xsiz,
                  ysiz
                );
            }
        }


        public Guppy2.GUI.Button AcceptButton
        {
            get
            {
                var wfbtn = Form.AcceptButton as Button;
                if (wfbtn == null) return null;
                return wfbtn.Tag as Guppy2.GUI.Button;
            }
            set
            {
                if (value == null)
                    Form.AcceptButton = null;
                else
                {
                    var ctrl=value.DriverControl.NativeObject as System.Windows.Forms.Button;
                    Form.AcceptButton =  ctrl;
                }
            }
        }


        private void UpdateWindowPlacement()
        {


            Window window = Form.Tag as Window;
            if (window == null) return;

            Rect2i oldbounds = null;
            Point2i pos = null;
            Size2i size = null;
            WindowSize initsize = window.InitialSize;
            WindowPosition initpos = window.InitialPosition;

            if (initsize == WindowSize.Minimal)
            {
                if (oldbounds == null)
                {
                    window.Refresh();
                    oldbounds = window.Bounds;
                }
                size = oldbounds.Size; // window.LayoutInfo.MinSize;
            }
            else if (initsize == WindowSize.Current)
            {
                if (oldbounds == null) 
                    oldbounds = window.Bounds;
                size = oldbounds.Size;
            }
            else
                throw new Exception("Unimplemented initial size");



            if (pos == null && initpos == WindowPosition.Current)
            {
                //if (oldbounds == null) oldbounds = Bounds;
                pos = new Point2i(oldbounds.X, oldbounds.Y);
            }
            if (pos == null && initpos == WindowPosition.CenterParent)
            {

                Form frm = Form.ActiveForm; //form to center to

                //Dont center invisible windows, this is actually a hack 
                //around centering to visual studios so called 'parking window' if this 
                //is the app:s first window shown
                if (frm!=null && frm.Visible == false) frm = null;

                //now we got a form to center to?
                if (frm != null)
                {
                    var fb = frm.Bounds;
                    int x = fb.Left + fb.Width / 2 - size.Width / 2;
                    int y = fb.Top + fb.Height / 2 - size.Height / 2;
                    pos = new Point2i(x, y);
                }
                else //no active window, fallback to screen center
                    initpos = WindowPosition.CenterScreen;
            }
            if (pos == null && initpos == WindowPosition.CenterScreen)
            {
                var rs = Guppy.ScreenResolution;
                int x = rs.Width / 2 - size.Width / 2;
                int y = rs.Height / 2 - size.Height / 2;

                pos = new Point2i(x, y);
            }
            if (pos == null) //unresolvable
                throw new Exception("Unimplemented initial position");

            //next time we show, use the current size and position
            window.InitialPosition = WindowPosition.Current;
            window.InitialSize = WindowSize.Current;

            Bounds = new Rect2i(pos, size);

        } //End UpdateWindowPlacement


        public Widget ActiveChild
        {
            get
            {
                Widget res = null;
                var ctrl = Form.ActiveControl;
                if (ctrl != null) //got an active control
                    res = ctrl.Tag as Widget;
                else  //no active control, return window itself
                    res = Form.Tag as Widget;

                return res;
            }
        }
    } //End class



    
    internal class CustomForm : Form
    {


        Window shellobject;

        public CustomForm(Window shellobj)
        {
            this.shellobject = shellobj;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            //we use this raw function catch tab key down etc
            KeyData kcode = WinFormsKeys.DecodeKey(keyData);
            if (kcode.KeyCode == KeyCode.Unknown)
                return base.ProcessCmdKey(ref msg, keyData);  //dont send events for unknown keys

            char oldchar = kcode.Char; //remember to check if user changed key stroke  TODO: does this work?


            KeyEventArgs ke = new KeyEventArgs(kcode);
            ((Window)shellobject).TriggerKeyPreview(ke);
            if (ke.Block) return true; //goodbye event

            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
} //End namespace

    


