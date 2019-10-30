using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2Test
{
    public class Prompt:IDisposable
    {

        string oldtext = string.Empty;  //the text of the previous prompt

        public Prompt(string text)
        {
            oldtext = Program.MainForm.StatusLabel.Caption;
            Program.MainForm.StatusLabel.Caption = text ?? string.Empty;
        }


        public void Dispose()
        {
            Program.MainForm.StatusLabel.Caption = oldtext;
        }
    }
}
