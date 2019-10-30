using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Guppy2.GFX.Windows;
using Guppy2.GFX;
using System.Runtime.InteropServices;

namespace Guppy2.GUI.WinForms
{
    public class GUIDriverWinForms:IGUIDriver
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct Message
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool PeekMessage(out Message message, IntPtr handle, uint filterMin, uint filterMax, uint flags);
        [DllImport("user32.dll")]
        static extern IntPtr DispatchMessage([In] ref Message lpmsg);
        [DllImport("user32.dll")]
        static extern bool TranslateMessage([In] ref Message lpMsg);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetMessage(out Message lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);


        public bool Open()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            return true;
        }

        public void Run()
        {
            Application.Run();
        }

        public void Quit()
        {
            Application.Exit();
        }

        public Size2i ScreenResolution
        {
            get
            {
                var s = Screen.PrimaryScreen.Bounds;
                return new Size2i(s.Width, s.Height);
            }
        }

        public Window ForegroundWindow
        {
            get
            {
                var frm = Form.ActiveForm;
                if (frm != null)
                    return frm.Tag as Window;
                return null;
            }
        }
        
        public IDriverButton CreateButton(GUIObject guiobject, string caption)
        {
            return new WinFormsButton(guiobject,caption);
        }

        public IDriverWindow CreateWindow(GUIObject guiobject, string caption,WindowStyle style)
        {
            return new WinFormsWindow(guiobject, caption,style);
        }

        public IDriverPanel CreatePanel(GUIObject guiobject)
        {
            return new WinFormsPanel(guiobject);
        }

        public IDriverFrame CreateFrame(GUIObject guiobject,string caption)
        {
            return new WinFormsFrame(guiobject, caption);
        }

        public IDriverToggle CreateToggle(GUIObject guiobject, string caption)
        {
            return new WinFormsToggle(guiobject, caption);
        }

        public IDriverEdit CreateEdit(GUIObject guiobject)
        {
            return new WinFormsEdit(guiobject);
        }

        public IDriverLabel CreateLabel(GUIObject guiobject,string caption)
        {
            return new WinFormsLabel(guiobject,caption);
        }

        public IDriverSeparator CreateSeparator(GUIObject guiobject, bool vertical)
        {
            return new WinFormsSeparator(guiobject, vertical);
        }


        internal static string MapTypefaceName(string name)
        {
            switch (name.ToUpper())
            {
                case "COURIER": return "Courier new";
                case "HELVETICA": return "Arial";
                case "TIMES": return "Times new roman";
                case "SYSFONT": return System.Drawing.SystemFonts.MessageBoxFont.Name;
                default: return name;
            }
        }

        public IDriverTypeface CreateTypeface(string name, double size, TypefaceStyle style)
        {
            return new WinFormsTypeface(MapTypefaceName(name), size, style);
        }

        public IDriverListBox CreateListBox(GUIObject guiobject)
        {
            return new WinFormsListBox(guiobject);
        }

        public IDriverChoice CreateChoice(GUIObject guiobj)
        {
            return new WinFormsChoice(guiobj);
        }

        public IDriverEditChoice CreateEditChoice(GUIObject guiobj)
        {
            return new WinFormsEditChoice(guiobj);
        }

        public IDriverProgressBar CreateProgressBar(GUIObject guiobj)
        {
            return new WinFormsProgressBar(guiobj);
        }

        public IDriverSlider CreateSlider(GUIObject guiobj,bool vertical)
        {
            return new WinFormsSlider(guiobj,vertical);
        }

        public IDriverTabs CreateTabs(GUIObject guiobj)
        {
            return new WinFormsTabs(guiobj);
        }

        public IDriverTabPage CreateTabPage(GUIObject guiobj, string caption)
        {
            return new WinFormsTabPage(guiobj, caption);
        }

        public IDriverSplitter CreateSplitter(GUIObject owner,bool vertical)
        {
            return new WinFormsSplitter(owner,vertical);
        }

        public IDriverCanvas CreateCanvas(GUIObject owner)
        {
            return new WinFormsCanvas(owner);
        }
        
        public Picture CreatePicture(Stream src,PictureMode mode)
        {
            if (mode == PictureMode.Hardware)
                return new PictureGDIPlus(src);
            else
                return new PictureWinRGB(src); 
        }

        public Picture CreatePicture(int width,int height, PictureMode mode)
        {
            if (mode == PictureMode.Hardware)
                return new PictureGDIPlus(width,height);
            else
                return new PictureWinRGB(width,height);
        }



        public void Wait(bool blocking)
        {
            Message msg;

            if (!blocking && !PeekMessage(out msg, IntPtr.Zero, 0, 0, 1)) //1=PM_NOREMOVE, do not remove message
                return; //not blocking and no message available

            if (GetMessage(out msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

    }
}
