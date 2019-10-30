using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class WidgetCollection:IEnumerable<Widget>
    {
        Group parent;
        List<Widget> children = new List<Widget>();

        public WidgetCollection(Group parent)
        {
            this.parent = parent;
        }

        public void Add(Widget child)
        {
            Detach(child);

            
            if(child.DriverControl!=null && parent.DriverGroup!=null)
                parent.DriverGroup.Append(child.DriverControl);
            child.Parent = parent;
            children.Add(child);
        }

        public Widget Detach(Widget child)
        {
            if (child.Parent != null)
            {
                int i = children.IndexOf(child);
                if (i < 0) return null;
                children.RemoveAt(i);
                if (child.DriverObject != null)
                    child.Parent.DriverGroup.Detach(child.DriverControl);
                child.Parent = null;
            }
            return child;
        }

        public IEnumerator<Widget> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return children.GetEnumerator();
        }

        public Widget this[int index] {
            get {
                return children[index];
            }
        }

        public int Count
        {
            get { return children.Count; }
        }
    }
}
