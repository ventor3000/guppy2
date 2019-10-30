using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI.ExtraWidgets
{
    public class Dialog : Window
    {
        public readonly Panel Contents;
        public readonly Panel Buttons;

        public Dialog(string caption, params string[] buttons)
            : base(caption, WindowStyle.FrameDialog)
        {
            Contents = new Panel(this) { Margin=Guppy.DefaultMargin }; //two step construction to avoid calling our own append
            new Separator(this, false) { ExpandX = true };
            Buttons = new Panel(this) {ChildrenUniformWidth = true, Vertical = false ,Align=Guppy2.GUI.Align.Center};

            Margin = Guppy.DefaultMargin;
            

            int nbut = buttons.Length / 2;
            for (int l = 0; l < nbut; l++)
                new Button(Buttons, buttons[l * 2]) { Response = buttons[l * 2 + 1] };

            EvKeyPreview += Dialog_EvKeyPreview; //to answer escape
        }

        void Dialog_EvKeyPreview(object sender,KeyEventArgs e)
        {
            if (e.KeyData.KeyCode == KeyCode.Escape)
                Close("");
        }
    }
}
