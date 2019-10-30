using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Guppy2.AppUtils
{
    public static class StreamUtils
    {

        /// <summary>
        /// Finds a resource in a given assembly given a string name. First it tries to match the resource name exactly. 
        /// If this fails, tries to match a resource, which ends with '.'+name, to make life simpler for VS Express users,
        /// becasue names are decorated by this IDE. If the resource search fails, null is returned, otherwise a stream
        /// to the resource is returned.
        /// </summary>
        /// <param name="resname"></param>
        /// <param name="asm"></param>
        /// <returns></returns>
        public static Stream FindResourceStream(string resname, Assembly asm)
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

            return stream;  //possibly null
        }


        /// <summary>
        /// Finds a resource name according to FindResourceStream(name,assembly), but using the assembly of
        /// the caller.
        /// </summary>
        public static Stream FindResourceStream(string resname)
        {
            return FindResourceStream(resname, Assembly.GetCallingAssembly());
        }

    }
}
