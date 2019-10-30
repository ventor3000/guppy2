using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class ListBox:Widget
    {

        public ListBox(Group parent,params object[] items)
        {
            Construct(Guppy.Driver.CreateListBox(this));
            if (parent != null)
                parent.Append(this);

            foreach (object i in items)
                Add(i,null);
        }


        private IDriverListBox DriverListBox {
            get {
                return DriverObject as IDriverListBox;
            }
        }

        public int Add(object obj,Picture image=null)
        {
            return DriverListBox.Add(obj,image);
        }

        public void Insert(int index, object obj, Picture image = null)
        {
            DriverListBox.Insert(index, obj, image);
        }

        public void Remove(int index)
        {
            int selidx = SelectedIndex;
            DriverListBox.Remove(index);

            //reselect for deleted selection
            if (selidx == index)
            {
                if (selidx >= Count)
                    selidx--;
                if (selidx >= 0)
                    SelectedIndex = selidx;
            }
        }
        
        public void Clear()
        {
            DriverListBox.Clear();
        }

      
        public int SelectedIndex
        {
            get
            {
                return DriverListBox.SelectedIndex;
            }

            set
            {
                DriverListBox.SelectedIndex = value;
            }
        }


        public bool Sorted
        {
            get
            {
                return DriverListBox.Sorted;
            }
            set
            {
                DriverListBox.Sorted = value;
            }
        }

        public int Count
        {
            get
            {
                return DriverListBox.Count;
            }
        }

        public bool MultiSelect
        {
            get
            {
                return DriverListBox.MultiSelect;
            }

            set { DriverListBox.MultiSelect = value; }
        }

        public bool GetSelected(int index)
        {
            return DriverListBox.GetSelected(index);
        }

        public void SetSelected(int index, bool selected)
        {
            DriverListBox.SetSelected(index, selected);
        }

        public int TopItem {
            set
            {
                DriverListBox.TopItem = value;
            }
        }

        public object this[int idx] {
            get {
                return DriverListBox[idx];
            }
        }

        public void SetImage(int idx, Picture img)
        {
            DriverListBox.SetPicture(idx, img);
        }

        public void SetObject(int idx, object obj)
        {
            DriverListBox.SetObject(idx, obj);
        }

        
    }
}
