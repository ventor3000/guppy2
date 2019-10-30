using Guppy2.Calc.Geom2d;
using Guppy2.GFX;
using Guppy2.GFX.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2Test
{
    public class Drawing
    {
        public Transform2d ViewTransform = Transform2d.Identity; //Tansforms from WCS to DCS

        public DB DB;

        OrderedDictionary<UInt64, EntitySpatial> Spatial = new OrderedDictionary<UInt64, EntitySpatial>(); //Geometric entities

        

        public Drawing()
        {
            DB = new DB();

            DB.Changed += DBChanged;


            EntityLine elin = new EntityLine(0, 0, 100, 100);
            DB.Add(elin);
            DB.Remove(elin);


            for (int l = 0; l < 10000; l++)
            {
                EntityLine el=new EntityLine(AppUtil.RandDouble(500), AppUtil.RandDouble(500), AppUtil.RandDouble(500), AppUtil.RandDouble(500));
                el.Color = RGB.Random;
                DB.Add(el);
            }
        }

        void DBChanged(object sender, DBEventArgs e)
        {


            switch (e.Type)
            {
                case DBEventType.Add:
                    if (e.Entity is EntitySpatial)
                        Spatial[e.Entity.DBID]=e.Entity as EntitySpatial; //TODO: check if modelspace block
                    break;
                case DBEventType.Remove:
                    if (e.Entity is EntitySpatial)
                        Spatial.Remove(e.Entity.DBID);
                    break;
                case DBEventType.Replace:
                    if (e.Entity is EntitySpatial)
                        Spatial[e.Entity.DBID] = e.Entity as EntitySpatial;
                    break;
            }
        }

        public void Draw(Painter painter)
        {
            painter.Transform = ViewTransform;

            foreach (EntitySpatial ent in Spatial.Values)
            {
                ent.Draw(painter);
            }
        }


       


    }
}
