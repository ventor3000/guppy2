using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using WF = System.Windows.Forms;
using SD = System.Drawing;
using Guppy2.GFX;
using Guppy2.GFX.Windows;

namespace Guppy2.GUI.WinForms
{
    public class WinFormsButton:WinFormsControl,IDriverButton
    {
        
        public WinFormsButton(GUIObject guiobject,string caption)
        {
            Init(new CustomButton(),guiobject);
            Caption = caption;

            Button.TextImageRelation = TextImageRelation.ImageBeforeText;
            //Button.TextAlign = ContentAlignment.MiddleLeft;
            Button.ImageAlign = ContentAlignment.MiddleRight;

            Button.Click += (s, e) =>
            {
                Button.Capture = false; //stupid winforms keeps capture while executing OnClick, avoid this
                ((Button)guiobject).TriggerClick(new EventArgs());
            };

            
        }

        private CustomButton Button { get { return Control as CustomButton; } }

        public override Size2i NaturalSize
        {
            get {


                Size s = TextRenderer.MeasureText(Caption, Button.Font);

                //special case:empty caption
                if (s.Height == 0)
                    s.Height = Button.Font.Height;
                   // s.Height = TextRenderer.MeasureText("W", button.Font).Height;

                if (Button.Image != null)
                {
                    s.Height = Math.Max(s.Height, Button.Image.Height);
                    s.Width += Button.Image.Width;
                }


                s.Width += SystemInformation.Border3DSize.Height * 2;
                s.Height += SystemInformation.Border3DSize.Width * 2;

                s.Width += 12;
                s.Height += 6;
                
                return new Size2i(s.Width, s.Height);
            }
        }


        public Picture Picture
        {
            set
            {
                if (value == null)
                    Button.Image = null;
                else
                {
                    Image img = null;
                    PictureGDIPlus gdiimg = value as PictureGDIPlus;
                    if (gdiimg != null)
                        img = gdiimg.Image;
                    
                    if (img != null)
                        Button.Image = img;
                }
            }
        }

        public bool Flat
        {
            get { return Button.Flat; }
            set { Button.Flat = value; }
        }


        public bool CanFocus
        {
            get
            {
                return Button.FocusEnabled;
            }
            set
            {
                Button.FocusEnabled = value;
            }
        }
                


        private class CustomButton : WF.Button
        {

            bool flat = false;


            public CustomButton()
            {
                FlatAppearance.BorderSize = 0;
            }


            public bool Flat
            {
                get
                {
                    return flat;
                }
                set
                {
                    FlatStyle = value ? System.Windows.Forms.FlatStyle.Flat : System.Windows.Forms.FlatStyle.Standard;
                    flat = value;
                }
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                if (flat)
                    FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                if (flat)
                    FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            }

            internal bool FocusEnabled //needed to be able to shut of focus
            {
                get
                {
                    return this.GetStyle(System.Windows.Forms.ControlStyles.Selectable);
                }
                set
                {
                    this.SetStyle(System.Windows.Forms.ControlStyles.Selectable, value);
                }
            }
        }
    }

    


}
