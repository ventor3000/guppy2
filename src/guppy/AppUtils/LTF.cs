using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Guppy2;
using System.Text.RegularExpressions;

namespace Guppy2.AppUtils
{

    public static class LTF
    {
        static List<string> catalogpaths=new List<string>();    //look up paths for catalogs
        static Dictionary<string, Catalog> catalogs = new Dictionary<string, Catalog>();    //all loaded catalogs
        static Catalog currentcatalog = null;
        
        /// <summary>
        /// Class to represent a single .mo file
        /// </summary>
        class Catalog
        {
            public Expression pluralexpr = null;    //expression to solve for plural index
            private Dictionary<string, CatalogEntry> Entries = new Dictionary<string, CatalogEntry>(); 

            internal string Get(string msgid)
            {
                CatalogEntry e;
                if (!Entries.TryGetValue(msgid, out e))
                    return msgid;
                return e.msgstrs[0];
            }

            internal string Get(string msgid, string msgid_plural, int n)
            {
                //figure out pluralindex
                int pluralidx=0;
                if (pluralexpr != null)
                {
                    pluralexpr.SetVar("n", n);

                    try
                    {
                        pluralidx = pluralexpr.EvalInt();
                    }
                    catch (Exception ex)
                    {

                    }
                }

                CatalogEntry e;

                if (!Entries.TryGetValue(msgid, out e))
                { //entry not found
                    if (n != 1)
                        return msgid_plural;
                    return msgid;
                }

                //check that plural index is not out of bounds
                int plurlen=e.msgstrs.Length;
                
                if(plurlen<=0)  //this should never happen
                    return msgid;

                if(pluralidx>0 && pluralidx<plurlen) 
                    return e.msgstrs[pluralidx];

                //plural index out of bounds....just return first one
                return e.msgstrs[0];
            }
            

            internal void Add(string[] msgids, string[] msgstrs)
            {
                //map with first msgid
                if (msgids.Length < 0)
                    return;
                Entries[msgids[0]]=new CatalogEntry(msgids,msgstrs);
            }

            class CatalogEntry
            {
                public CatalogEntry(string[] msgids, string[] msgstrs)
                {
                    this.msgids = msgids;
                    this.msgstrs = msgstrs;
                }
                public string[] msgids;
                public string[] msgstrs;
            }
        }


        public static void Initialize()
        {
            catalogpaths.Clear();
            catalogs.Clear();
            currentcatalog = null;
            AddCatalogPath(StandardPaths.LocalizedMessages);
        }

        public static void AddCatalogPath(string path)
        {
            if (catalogpaths.IndexOf(path) >= 0)
                return; //already exists
            catalogpaths.Add(path);
        }

        /// <summary>
        /// Adds a catalog to the message catalogs. If the catalog was found and loaded,
        /// true is returned, otherwise false. An exception is thrown only if the message object file
        /// existed but was invalid/corrupt. LTF.Initialize() is called if not already done.
        /// </summary>
        /// <param name="domainname">The name of the message domain</param>
        public static bool AddCatalog(string domainname)
        {
            if (!Initialized)
                Initialize();

            //Try to find the physical message object file
            foreach (string path in catalogpaths)
            {
                string mopath = Path.Combine(path, domainname + ".mo");
                if (File.Exists(mopath))
                {
                    Catalog c = LoadMO(mopath);
                    if (c != null)
                        catalogs[domainname] = c;
                    if (currentcatalog == null)
                        currentcatalog = c;
                    return true;
                }
            }

            return false;
        }

        public static bool Bind(string domain)
        {
            Catalog c;
            if (catalogs.TryGetValue(domain, out c))
            {
                currentcatalog = c;
                return true;
            }

            return false;
        }



        private static bool FindCatalog(string domain, out Catalog c)
        {
            c = null;
            if (domain == null)
            {
                if (currentcatalog == null)
                    return false;   //no current catalog
                c = currentcatalog;
            }
            else if (!catalogs.TryGetValue(domain, out c))
                return false; //cant find catalog of that domain
            
            return true;
        }
        
        public static string GetString(string msgid, string domain=null)
        {
            Catalog c;
            if (!FindCatalog(domain, out c)) return msgid;
            return c.Get(msgid);
        }

        public static string _(string msgid, string domain = null)
        {
            return GetString(msgid, domain);
        }
      

        public static string GetPluralString(string msgid, string msgid_plural, int n,string domain=null)
        {
            Catalog c;
            if (!FindCatalog(domain, out c)) return msgid;
            return c.Get(msgid,msgid_plural,n);
        }

        public static bool Initialized
        {
            get
            {
                return catalogpaths.Count > 0;
            }
        }


        private struct StringPosition
        {
            public int Offset;
            public int Length;
            public string[] Strings;

            public StringPosition(int len, int offs)
            {
                this.Offset = offs;
                this.Length = len;
                Strings = null;
            }



            public override string ToString()
            {
                return "StringPosition, offset=" + Offset.ToString() + " length=" + Length.ToString();
            }
        }

        static string[] ReadEncodedStrings(BinaryReader br, int numbytes,Encoding enc)
        {
            byte[] bytes = br.ReadBytes(numbytes);
            string res=enc.GetString(bytes);
            string[] forms = res.Split('\0');
            return forms;
        }


        static void ParseMOHeader(string headerstr,out string charset,out string plural)
        {
            Regex re = new Regex(@"charset\s*=\s*(.*)",RegexOptions.IgnoreCase);

            Match m;
            m=re.Match(headerstr, 0);
            if (m.Success)
                charset = m.Groups[1].Value;
            else
                charset = "utf-8";  //assume this
            
            re = new Regex(@"plural\s*=\s*([^;]*)",RegexOptions.IgnoreCase);
            m = re.Match(headerstr);
            if (m.Success)
                plural = m.Groups[1].Value;
            else
                plural = "n != 1";
        }
        
        static Catalog LoadMO(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            uint magic = br.ReadUInt32();
            if (magic != 0x950412de && magic != 0xde120495)
                throw new InvalidDataException("Magic number of .MO data is incorrect");
            int revision = br.ReadInt32();
            if (revision != 0 && revision != 1)
                throw new Exception("Only .mo files revision 0 and 1 supported (This has revision " + revision.ToString() + ")");

            uint numstrings = br.ReadUInt32();
            uint orgstr_offset = br.ReadUInt32();
            uint transstr_offset = br.ReadUInt32();
            br.ReadUInt32(); //hash size
            br.ReadUInt32(); //hash offset

            StringPosition[] org_strings = new StringPosition[numstrings];
            StringPosition[] tr_strings = new StringPosition[numstrings];

            

            //read in length+offsets of original string
            stream.Seek(orgstr_offset, SeekOrigin.Begin);
            for (int l = 0; l < numstrings; l++) {
                org_strings[l] = new StringPosition(br.ReadInt32(), br.ReadInt32());
            }

            //read in length+offsets of translated strings string
            stream.Seek(transstr_offset, SeekOrigin.Begin);
            for (int l = 0; l < numstrings; l++)
                tr_strings[l] = new StringPosition(br.ReadInt32(), br.ReadInt32());


            Catalog c = new Catalog();


            Encoding enc = Encoding.UTF8;
            //read in the actual strings
            for (int l = 0; l < numstrings; l++)
            {

                stream.Seek(org_strings[l].Offset, SeekOrigin.Begin);
                string[] orgstrings = ReadEncodedStrings(br, org_strings[l].Length,enc);
                org_strings[l].Strings = orgstrings;
                
                stream.Seek(tr_strings[l].Offset, SeekOrigin.Begin);
                string[] transstrs=ReadEncodedStrings(br, tr_strings[l].Length,enc);
                c.Add(orgstrings, transstrs);
                
                if (orgstrings[0] == "")
                {
                    string encoding, plural;
                    ParseMOHeader(transstrs[0],out encoding,out plural);
                    try
                    {
                        enc = Encoding.GetEncoding(encoding);
                    }
                    catch(Exception) {
                        enc=Encoding.UTF8;  //non supported encoding, go for utf8 and wish the best...
                    }

                    try
                    {
                        c.pluralexpr = new Expression(plural);
                    }
                    catch
                    {
                        c.pluralexpr = null;
                    }
                }
            }

            return c;            
        }
        

        static Catalog LoadMO(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var res=LoadMO(fs);
            fs.Close();
            return res;
        }
    }
}

