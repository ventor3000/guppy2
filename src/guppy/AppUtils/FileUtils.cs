using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Guppy2.AppUtils
{
    public static class FileUtils
    {
        public static bool FileExists(string path,int timeout=3000)
        {
            try
            {
                return File.Exists(path);   // TODO: support timeout
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DirectoryExists(string path,int timeout=3000)
        {
            try
            {
                return Directory.Exists(path); // TODO: support timeout
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
