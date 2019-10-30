using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WF = System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{
    class WinFormsEdit:WinFormsControl,IDriverEdit
    {

        public WinFormsEdit(GUIObject guiobject)
        {
            Init(new WF.TextBox(), guiobject);

            //TODO: make possible to change the typed key!

            TextBox.KeyDown += (s, e) => {
                var be = new KeyEventArgs(WinFormsKeys.DecodeKey(e.KeyCode));
                ((Edit)Widget).TriggerKeyDown(be);
                e.SuppressKeyPress = be.Block;               
            };
            
            
        }



        

        public string Text
        {
            get
            {
                return TextBox.Text;
            }
            set
            {
                TextBox.Text = value;
            }
        }

        private WF.TextBox TextBox { get { return Control as WF.TextBox; } }

        public override Size2i NaturalSize
        {
            get {
                var pf=TextBox.PreferredSize;
                int w = Guppy.DefaultEditWidth;
                int h = pf.Height;
                return new Size2i(w, h);

            }
        }
    }
}
