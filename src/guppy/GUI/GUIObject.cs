using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class GUIObject:IDisposable
    {
        internal IDriverGUIObject DriverObject;

        ~GUIObject()
        {
            Dispose();
        }

        virtual protected void Construct(IDriverGUIObject obj)
        {
            this.DriverObject = obj;
        }

        public object NativeObject { get { return DriverObject.NativeObject; } }

        public virtual void Dispose()
        {
            IDisposable idisp = DriverObject as IDisposable;
            if(idisp!=null)
                idisp.Dispose();
        }

    }
}
