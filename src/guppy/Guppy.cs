using Guppy2.GFX;
using Guppy2.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Guppy2
{
    public enum Platform {
        Unknown,
        Windows,
        Linux,
        MacOSX
    }
    

    public static class Guppy
    {
        private static IGUIDriver _driver=null;
        private static Window _mainwindow = null;


        //default values used in gui layout
        public static readonly int DefaultEditWidth=75;
        public static readonly int DefaultGap = 4;
        public static readonly int DefaultListBoxWidth=75;
        public static readonly int DefaultListBoxHeight=120;
        public static readonly int DefaultTrackbarLength = 120;
        public static readonly Margin DefaultMargin = new Margin(8, 8, 8, 8);
        

        public static void Open()
        {
            _driver = new Guppy2.GUI.WinForms.GUIDriverWinForms();
            _driver.Open();
        }

        public static void Run(Window mainwindow) {
            //Guppy.mainwindow = mainwindow;
            _mainwindow = mainwindow;

            mainwindow.EvClosed += (sender,e) =>
            {
               Driver.Quit();  //terminate application when mainwindow closes
            };

            mainwindow.Show();
            Driver.Run();
        }

        public static Window MainWindow { get { return _mainwindow; } }
        public static Size2i ScreenResolution { get { return Driver.ScreenResolution; } }
        public static Window ForegroundWindow { get { return Driver.ForegroundWindow; } }
        

        public static IGUIDriver Driver
        {
            get
            {
                if (_driver == null)
                {
                    Guppy.Open(); //not opened, do a panic try
                    if (_driver == null)
                    {
                        throw new Exception("Guppy is not Open():ed, this must be done before using guppy");
                    }
                }
                return _driver;
            }
            internal set
            {
                if (_driver != null)
                    throw new Exception("Guppy driver cannot be changed once set");
                _driver = value;
            }
        }

        public static Platform Platform
        {
            get
            {
                PlatformID plat = System.Environment.OSVersion.Platform;
                switch (plat)
                {
                    case PlatformID.Win32NT: return Platform.Windows;
                    case PlatformID.MacOSX: return Platform.MacOSX;
                    case PlatformID.Unix: return Platform.Linux;
                    default: return Platform.Unknown;
                }

            }
        }

        public static Picture CreatePicture(Stream source, PictureMode mode = PictureMode.Hardware) {
            return Driver.CreatePicture(source,mode);
        }

        public static Picture CreatePicture(int width, int height, PictureMode mode = PictureMode.Hardware)
        {
            return Driver.CreatePicture(width, height, mode);
        }

        public static Picture CreatePicture(string filename, PictureMode mode = PictureMode.Hardware)
        {

            Picture res;
            FileStream fs = File.OpenRead(filename);
            try
            {
                res = CreatePicture(fs,mode);
            }
            finally
            {
                fs.Close();
            }
            return res;
        }


        public static void Wait(bool blocking)
        {
            Driver.Wait(blocking);
        }
    }
}
