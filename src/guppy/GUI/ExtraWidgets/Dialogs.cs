using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI.ExtraWidgets
{
    public class Dialogs
    {
        public static void Message(string caption, string msg)
        {
            using (Dialog d = new Dialog(caption,"Ok","OK") /*{ Gap = 16 }*/)
            {
                new Label(d.Contents, msg) {  };
                d.Popup();
            }
        }

        public static bool YesNo(string caption, string msg,bool def=false)
        {
            using (Dialog d = new Dialog(caption,"Yes","YES","No","")/* { Gap = 16 }*/)
            {
                var dc = d.Contents;
                new Fill(dc) { Height = 16 };
                new Label(dc, msg);
                new Fill(dc) { Height = 16 };
                var btns = new Panel(dc) { Vertical = false };
                
                if (def)
                    d.AcceptButton = d.Buttons[0] as Button;
                else
                    d.AcceptButton = d.Buttons[1] as Button;
                

                if (d.Popup() == "YES")
                    return true;
                return false;
            }
        }




    }
}
