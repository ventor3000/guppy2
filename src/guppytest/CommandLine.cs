using Guppy2.Calc.Geom2d;


namespace Guppy2Test
{
    public class CommandLine
    {

        internal static void Run()
        {
            Point2d prevpt=TC.GetPoint("Specify first point");
            Point2d nextpt=TC.GetPoint("Specify next point",prevpt);

            EntityLine eli = new EntityLine(prevpt.X, prevpt.Y, nextpt.X, nextpt.Y);


            TC.AddEntity(eli);
        }
    }
}
