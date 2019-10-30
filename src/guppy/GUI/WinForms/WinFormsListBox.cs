using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using WF = System.Windows.Forms;

namespace Guppy2.GUI.WinForms
{

  
    internal class ListItem
    {

        public Picture Picture; //null fo no image
        public object Data;

        public ListItem(object data, Picture pic)
        {
            this.Picture = pic;
            this.Data = data;
        }

        public override string ToString()
        {
            if (Data == null)
                return "";
            return Data.ToString();
        }
    }

    public class WinFormsListBox : WinFormsControl,IDriverListBox
    {
        public WinFormsListBox(GUIObject guiobject)
        {
            Init(new WF.ListBox(), guiobject);
            ListBox.IntegralHeight = false;
            
            ListBox.DrawMode = WF.DrawMode.OwnerDrawFixed;
            ListBox.DrawItem += ListBox_DrawItem;
        }

        void ListBox_DrawItem(object sender, WF.DrawItemEventArgs e)
        {
            e.DrawBackground();


            ListItem obj = GetListItem(e.Index);
            if (obj == null)
                return;
            
            Rectangle bounds = e.Bounds;
            int textleft = (int)bounds.Left;
            string str=obj.ToString();

            if (obj.Picture!=null)
            {
                var img = obj.Picture.NativeObject as System.Drawing.Image;
                if (img != null)
                {
                    //e.Graphics.DrawImage(img, (float)bounds.Left, bounds.Top+bounds.Height/2f-texsiz.Height/2f);
                    e.Graphics.DrawImage(img, (float)bounds.Left+2, bounds.Top + bounds.Height/2f-img.Height/2f,img.Width,img.Height);
                    textleft += img.Width + 4;
                }
            }

            /*e.Graphics.DrawString(obj.ToString(),
                e.Font,Brushes.Black, textleft,bounds.Top, StringFormat.GenericDefault);*/
            if (str != null)
            {
                Brush textbrush = (e.State & WF.DrawItemState.Selected) != 0 ? SystemBrushes.HighlightText : SystemBrushes.WindowText;
                SizeF texsiz = e.Graphics.MeasureString(str, e.Font);
                e.Graphics.DrawString(str, e.Font,textbrush,(float)textleft, bounds.Top + bounds.Height / 2f - texsiz.Height / 2f);
            }
            
            e.DrawFocusRectangle();
        }

        

        public override Size2i NaturalSize
        {
            get { return new Size2i(Guppy.DefaultListBoxWidth, Guppy.DefaultListBoxHeight); }
        }

        private WF.ListBox ListBox
        {
            get {
                return Control as WF.ListBox;
            }
        }

        public int Add(object obj,Picture image)
        {
            int res=ListBox.Items.Add(new ListItem(obj,image));
            GrowItemHeight(image);
            return res;
        }

        void GrowItemHeight(Picture i)
        {
            if (i == null)
                return;

            ListBox.ItemHeight = Math.Max(i.Height + 4, ListBox.ItemHeight);
        }


        public void Insert(int index,object item,Picture pic)
        {
            ListBox.Items.Insert(index,new ListItem(item,pic));
            GrowItemHeight(pic);
        }

       
        
        public void Clear()
        {
            ListBox.Items.Clear();
        }

        public int SelectedIndex 
        {
            get
            {
                return ListBox.SelectedIndex;
            }
            set
            {
                ListBox.ClearSelected();
                ListBox.SelectedIndex = value;
            }
        }


        public bool AutoHideScroll
        {
            get
            {
                return ListBox.ScrollAlwaysVisible;
            }
            set
            {
                ListBox.ScrollAlwaysVisible = !value;
            }
        }

        public int Count
        {
            get
            {
                return ListBox.Items.Count;
            }
        }


        public void Remove(int idx)
        {
            if (idx < 0 || idx >= Count)
                return;
            ListBox.Items.RemoveAt(idx);
        }

        public bool Sorted
        {
            get { return ListBox.Sorted; }
            set { ListBox.Sorted = value; }
        }

        public bool MultiSelect {
            get {return ListBox.SelectionMode==WF.SelectionMode.MultiExtended;}
            set { ListBox.SelectionMode = value ? WF.SelectionMode.MultiExtended : WF.SelectionMode.One; }
        }


        public bool GetSelected(int index)
        {
            return ListBox.GetSelected(index);
        }

        public void SetSelected(int index, bool selected)
        {
            ListBox.SetSelected(index, selected);
        }


        public int TopItem
        {
            set
            {
                ListBox.TopIndex = value; //TODO: this seems not to work, why?
            }
        }

        private ListItem GetListItem(int idx)
        {
            if (idx < 0 || idx >= Count)
                return null;
            var li = ListBox.Items[idx] as ListItem;
            return li;
        }

        public object this[int idx]
        {
            get
            {
                var li = GetListItem(idx);
                if (li == null) return null;
                return li.Data;
            }
        }


        public void SetPicture(int index, Picture pic)
        {
            var li = GetListItem(index);
            if (li == null) return;
            li.Picture = pic;

            GrowItemHeight(pic);
            /*if(image!=null) //grow item height
                ListBox.ItemHeight = Math.Max(image.Height + 4, ListBox.ItemHeight);*/

            ListBox.Invalidate();
        }

        public void SetObject(int index, object obj)
        {
            var li = GetListItem(index);
            if (li == null) return;

            li.Data = obj;

            ListBox.Invalidate();
         
        }
        
    }
}
