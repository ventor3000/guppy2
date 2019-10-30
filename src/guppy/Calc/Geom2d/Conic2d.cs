using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc.Geom2d
{
    public abstract class Conic2d:Curve2d
    {
        public abstract GeneralConic2d ToGeneralConic();
    }
}
