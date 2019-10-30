using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2Test
{
    public class Entity
    {
        public delegate Entity CreateEntityDelegate(Dictionary<string,object> data);
        static Dictionary<string,CreateEntityDelegate> EntityCreateFuncs=new Dictionary<string,CreateEntityDelegate>();
        public UInt64 DBID = 0; //id of this entity in database, or null if not inserted in a db


        public static void Register(string entname, CreateEntityDelegate createfunc)
        {
            EntityCreateFuncs.Add(entname, createfunc);
        }

        public static void Make(string entname, Dictionary<string, object> data)
        {
            CreateEntityDelegate fn;
            if (!EntityCreateFuncs.TryGetValue(entname, out fn))
                throw new Exception("Dont know how to make entity of type '" + entname + "'");
            fn(data);
        }

        public virtual void Create(Entity ent,Dictionary<string,object> data) {


        }
    }
}
