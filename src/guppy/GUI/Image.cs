using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Guppy2.GFX;

namespace Guppy2.GUI
{

    /*

    public class Image:GUIObject
    {
        internal Image(IDriverImage drvimg,ImageMode style)
        {
            this.DriverObject = drvimg;
        }

        private Picture DriverPicture
        {
            get
            {
                return DriverObject as Picture;
            }
        }

        public static Image FromFile(string filename,ImageMode mode=ImageMode.Hardware)
        {
            Image res = null;

            FileStream fs = File.OpenRead(filename);
            try
            {
                res = FromStream(fs);
            }
            finally
            {
                fs.Close();
            }
            return res;
        }

        public static Image FromStream(Stream stream, ImageMode mode = ImageMode.Hardware)
        {

            try
            {
                IDriverImage di = Guppy.Driver.CreatePicture(stream,mode);
                return new Image(di,mode);
            }
            catch (Exception ex)
            {
                return null;
            }
            

            
        }

        public static Image FromResource(Assembly asm, string resname)
        {

            

            //try to get resource with the actual name
            Stream stream = asm.GetManifestResourceStream(resname);
            if (stream == null)
            {
                //get resource with some random namspace ending with the given name
                //this is good for Visual Studio express users, where resources are auto named
                string upname = "." + resname.ToUpper();
                foreach (string s in asm.GetManifestResourceNames())
                {
                    if (s.ToUpper().EndsWith(upname))
                    {
                        stream = asm.GetManifestResourceStream(s);
                        break;
                    }
                }
            }

            if (stream == null)
                throw new Exception("Resource " + resname + " not found in assembly " + asm.ToString());

            Image res = null;
            try
            {
                res = FromStream(stream);
            }
            finally
            {
                stream.Close();
            }

            return res;

        }

        public static Image FromResource(string resname)
        {
            return FromResource(Assembly.GetCallingAssembly(), resname);
        }


        public int Width
        {
            get { return DriverPicture.Width; }
        }

        public int Height
        {
            get
            {
                return DriverPicture.Height;
            }
        }

        public Picture Picture
        {
            get
            {
                return DriverPicture;
            }
        }
    }*/
}
