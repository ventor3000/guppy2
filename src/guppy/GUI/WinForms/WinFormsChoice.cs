using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WF = System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{
    public class WinFormsChoice:WinFormsControl,IDriverChoice
    {

        private Size2i cached_natural_size = null;
        private const int image_text_spacing = 2;

        public WinFormsChoice(GUIObject guiobject)
        {
            Init(new WF.ComboBox(), guiobject);

            ComboBox.DropDownStyle = WF.ComboBoxStyle.DropDownList;
            ComboBox.DrawMode = WF.DrawMode.OwnerDrawFixed;
            ComboBox.DrawItem += ComboBox_DrawItem;

            

            /*ListBox.DrawMode = WF.DrawMode.OwnerDrawFixed;
            ListBox.DrawItem += ListBox_DrawItem;*/
        }

        void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {

            

            e.DrawBackground();


            ListItem obj = GetItem(e.Index);
            if (obj == null)
                return;

            Rectangle bounds = e.Bounds;
            int textleft = (int)bounds.Left;
            string str = obj.ToString();

            if (obj.Picture != null)
            {
                var img = obj.Picture.NativeObject as System.Drawing.Image;
                if (img != null)
                {
                    
                    e.Graphics.DrawImage(img, (float)bounds.Left + image_text_spacing, bounds.Top + bounds.Height / 2f - img.Height / 2f, img.Width, img.Height);
                    textleft += img.Width + image_text_spacing*2;
                }
            }

            if (str != null)
            {
                Brush textbrush = (e.State & WF.DrawItemState.Selected) != 0 ? SystemBrushes.HighlightText : SystemBrushes.WindowText;
                SizeF texsiz = e.Graphics.MeasureString(str, e.Font);
                e.Graphics.DrawString(str, e.Font, textbrush, (float)textleft, bounds.Top + bounds.Height / 2f - texsiz.Height / 2f);
            }

            e.DrawFocusRectangle();
        }

        private ListItem GetItem(int idx)
        {
            if (idx < 0 || idx >= ComboBox.Items.Count)
                return null;
            var li = ComboBox.Items[idx] as ListItem;
            return li;
        }

        protected WF.ComboBox ComboBox
        {
            get
            {
                return Control as WF.ComboBox;
            }
        }

        public int Add(object obj, Picture image)
        {
            int res = ComboBox.Items.Add(new ListItem(obj, image));
            if(image!=null)
                ComboBox.ItemHeight = Math.Max(ComboBox.ItemHeight, image.Height);
            cached_natural_size = null; //need recompute
            return res;
        }

        public override Size2i NaturalSize
        {
            get {


                if (cached_natural_size == null)
                {


                    int height = ComboBox.Height; //assume button is aproximately square

                    int itemwidth = 20;//minimum width

                    Font font = ComboBox.Font;
                    foreach (ListItem item in ComboBox.Items)
                    {
                        Size itemsize = TextRenderer.MeasureText(ComboBox.GetItemText(item.Data), font);

                        int i=itemsize.Width;

                        if (item.Picture != null)
                            i += item.Picture.Width + image_text_spacing * 2;

                        itemwidth = Math.Max(itemwidth,i );
                    }

                    

                    cached_natural_size = new Size2i(itemwidth + WF.SystemInformation.VerticalScrollBarWidth, height); //assume scrollbar width to be the size of drop down button
                }

                return cached_natural_size;
            }
        }
    }
}
