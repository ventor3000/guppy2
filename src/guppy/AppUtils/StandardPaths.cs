using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Globalization;

namespace Guppy2
{
    public static class StandardPaths
    {
        //TODO: make sure backslash is appended to all paths end extend this class. Prefix path on linux?

        public static string Application
        {
            get {
                return Path.GetDirectoryName( Assembly.GetEntryAssembly().Location);
            }
        }

        public static string LocalizedMessages
        {
            get
            {
                string lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                switch (Guppy.Platform)
                {
                    case Platform.Windows: return Path.Combine(Application, lang);
                    default: return Path.Combine(".", lang);    //TODO: support mac and *nix
                }
            }
        }
    }
}
