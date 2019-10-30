using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2Test
{

    public enum DBEventType {
        Add,
        Remove,
        Replace
    }

    public class DBEventArgs:EventArgs
    {

        public DBEventArgs(DBEventType type, Entity ent, Entity oldent)
        {
            this.Type = type;
            this.Entity = ent;
            this.ReplacedEntity = oldent;
        }

        internal DBEventType Type;          // Type of operation
        internal Entity Entity;             // Entity itself
        internal Entity ReplacedEntity;     // The entity that was replace if Type id Replace
    }

    public delegate void DBEvent(object sender, DBEventArgs e);

    /*
    public class EntHandle
    {
        internal UInt64 DBHandle; //0 is a null/none handle
        internal DB Owner;    //the database that owns this handle or null if none

        public static EntHandle Null = new EntHandle(null, 0);

        internal EntHandle(DB owner, UInt64 id)
        {
            this.Owner = owner;
            this.DBHandle = id;
        }

        public override int GetHashCode()
        {
            return DBHandle.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            EntHandle eh = obj as EntHandle;
            if (eh != null)
                return eh.DBHandle == this.DBHandle;
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return "Entity handle: " + DBHandle.ToString();
        }
    }*/


    

    public class DB
    {
        static UInt64 idcounter = 1; //used to generate unique db id:s
        OrderedDictionary<UInt64, Entity> entities = new OrderedDictionary<UInt64, Entity>();
        public event DBEvent Changed;

        public UInt64 Add(Entity ent)
        {

            if (ent.DBID > 0)
                throw new Exception("Cannot insert entity that is already inserted in a DB");

            ent.DBID = idcounter++;


            if (Changed != null)
                Changed(this, new DBEventArgs(DBEventType.Add, ent, null));
            
            entities[ent.DBID] = ent;
            return ent.DBID;
        }

        public void Remove(Entity ent)
        {
            UInt64 dbid = ent.DBID;
            if (dbid <= 0)
                throw new Exception("Cannot remove ent from DB because it is not inserted");

            if (Changed != null)
                Changed(this, new DBEventArgs(DBEventType.Remove, ent, null));

            entities.Remove(dbid);
            ent.DBID = 0;
        }

        
        public void Replace(Entity existing, Entity newent)
        {
            if (existing == null || existing.DBID <= 0)
            {
                Add(newent);
            }
            else
            {
                UInt64 dbid = existing.DBID;
                if (dbid <= 0)
                    throw new Exception("Cannot replace ent in DB because it is not inserted");
                if (newent.DBID>0)
                    throw new Exception("Cannot insert newent into DB because it is already inserted in a DB");

                if (Changed != null)
                    Changed(this, new DBEventArgs(DBEventType.Replace, existing, newent));
                
                entities[existing.DBID] = newent;
                newent.DBID = existing.DBID;
                existing.DBID = 0;  //now not inserted anymore
            }
        }


        public override string ToString()
        {
            return entities.ToString();
        }
        
    }


    public class OrderedDictionary<K, V>
    {
        internal LinkedList<V> Values=new LinkedList<V>();
        Dictionary<K, LinkedListNode<V>> lut=new Dictionary<K,LinkedListNode<V>>();

        public void Add(K key, V item)
        {
            if (lut.ContainsKey(key))
                throw new Exception("Trying to add already existing item to Ordered dictionary");
            LinkedListNode<V> nod = Values.AddLast(item);
            lut[key] = nod;
        }

        public void Remove(K key)
        {
            if (!lut.ContainsKey(key))
                throw new Exception("Trying to remove non existing item from DB:" + key.ToString());
            LinkedListNode<V> llnod = lut[key];
            Values.Remove(llnod);
            lut.Remove(key);
        }

        public V this[K key] {
            get
            {
                return lut[key].Value;
            }
            set
            {
                LinkedListNode<V> itv;
                if (lut.TryGetValue(key, out itv))
                { //already existing. replace
                    itv.Value = value;
                }
                else
                {
                    itv = Values.AddLast(value);
                    lut[key] = itv;
                }
            }
        }


        public override string ToString()
        {
            return "Count: " + Values.Count.ToString();
        }
        
    }


        
}
