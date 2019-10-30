using Guppy2.AppUtils;
using Guppy2.Calc;
using Guppy2.Calc.Geom2d;
using Guppy2.GFX.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Guppy2.GFX
{

    public enum PaperSize
    {
        A0,
        A1,
        A2,
        A3,
        A4,
        A5,
        Letter,
        Legal
    }

    /// <summary>
    /// Class to keep track of which attributes was changed, so that we dont
    /// write each change to file, if not needed. The Update... functions of this class
    /// realizes the changes done to file.
    /// </summary>
    internal class PDFAttributeState
    {

        public bool ChangedLineWidth = true; //true because this need to be changed on new page
        public double LineWidth = 0.0;

        public bool ChangedColor = false;
        public int Color = RGB.Black;

        public bool ChangedEndCap = false;
        public EndCap EndCap = EndCap.Round;

        public bool ChangedLineJoin = true; //bevel join is not default in pdf, force it
        public LineJoin LineJoin = LineJoin.Bevel;

        public bool ChangedTransform = false;
        public Transform2d Transform = Transform2d.Identity;

        public bool ChangedHatch = false;
        public Hatch Hatch = Hatch.Cross;

        public bool ChangedFillStyle = false;
        public FillStyle FillStyle = FillStyle.Solid;

        public bool ChangedLineStyle = false;
        public LineStyle LineStyle = LineStyle.Continuous;

        public bool ChangedLineStyleDashes = false;
        public double[] LineStyleDashes = Painter.DefaultLineStyleDashes;

        public bool ChangedPattern = false;
        public Picture Pattern = null;

        public bool ChangedClip = false;
        public Rectangle2i Clip = null;

        public bool ChangedPatternTransform = false;
        public Transform2d PatternTransform = Transform2d.Identity;

        /*public bool ChangedFillMode = false;
        public FillMode FillMode = FillMode.EvenOdd;*/

        public bool ChangedOpacity = false;
        public double Opacity = 1.0;



        public void UpdateStrokeColor(PDFObjectStream Contents)
        {
            if (ChangedColor)
            {
                ChangedColor = false;


                
                if (FillStyle == FillStyle.Hatch) //modify color on hatch pattern
                {
                    ChangedFillStyle = true;
                    UpdateFillStyle(Contents);
                    return;
                }

                //if (FillStyle == FillStyle.Solid)
                { //ie. not on pattern or hatch
                    //pdf differs from fill and stroke color (which not postscript do!) so we write both
                    string colstr = PainterPDF.RGBString(Color);
                    //Contents.WriteLine(colstr + " rg");
                    Contents.WriteLine(colstr + " RG");
                }
            }
        }


        public void UpdateLineWidth(PDFObjectStream Contents)
        {
            if (ChangedLineWidth)
            {
                ChangedLineWidth = false;
                Contents.WriteLine(PainterPDF.RTOS(LineWidth) + " w");
            }
        }

        public void UpdateEndCaps(PDFObjectStream Contents)
        {
            if (ChangedEndCap)
            {
                ChangedEndCap = false;
                switch (EndCap)
                {
                    case EndCap.Flat: Contents.WriteLine("0 J"); break;
                    case EndCap.Round: Contents.WriteLine("1 J"); break;
                    case EndCap.Square: Contents.WriteLine("2 J"); break;
                }
            }
        }


        public void UpdateLineJoin(PDFObjectStream contents)
        {

            if (ChangedLineJoin)
            {
                ChangedLineJoin = false;

                switch (LineJoin)
                {
                    case LineJoin.Bevel: contents.WriteLine("2 j"); break;
                    case LineJoin.Miter: contents.WriteLine("0 j"); break;
                    case LineJoin.Round: contents.WriteLine("1 j"); break;
                }
            }
        }

        public void UpdateLineStyle(PDFObjectStream contents)
        {

            if (ChangedLineStyle)
            {
                ChangedLineStyle = false;

                double mmperpixel = 0.35277777;
                double dashlen = mmperpixel * 24.0;
                double dotlen = mmperpixel * 6.0;

                switch (LineStyle)
                {

                    case LineStyle.Dash: contents.WriteLine(string.Format("[{0} {0}] 0 d", PainterPDF.RTOS(dashlen))); break;
                    case LineStyle.Dot: contents.WriteLine(string.Format("[{0} {0}] 0 d", PainterPDF.RTOS(dotlen))); break;
                    case LineStyle.DashDotDot: contents.WriteLine(string.Format("[{0} {1} {1} {1} {1} {1}] 0 d", PainterPDF.RTOS(dashlen), PainterPDF.RTOS(dotlen))); break;
                    case LineStyle.DashDot: contents.WriteLine(string.Format("[{0} {1} {1} {1}] 0 d", PainterPDF.RTOS(dashlen), PainterPDF.RTOS(dotlen))); break;
                    case LineStyle.Custom: contents.WriteLine("[" + PainterPDF.RealsToString(LineStyleDashes) + "] 0 d"); break;
                    default: /*continuous*/ contents.WriteLine("[] 0 d"); break;
                }
            }
        }

        public void UpdateFillStyle(PDFObjectStream contents)
        {

            if (ChangedFillStyle)
            {

                ChangedFillStyle = false;

                if (FillStyle == FillStyle.Solid)
                {
                    string colstr = PainterPDF.RGBString(Color);
                    contents.WriteLine(colstr + " rg");
                    contents.WriteLine(colstr + " RG");
                }
                else if (FillStyle == FillStyle.Hatch)
                {
                    var w = contents.writer;


                    PDFObjectPattern pat;

                    switch (w.Hatch)
                    {
                        case Hatch.BackwardDiagonal: pat = w.HatchBackwardDiag(Color); break;
                        case Hatch.ForwardDiagonal: pat = w.HatchForwardDiag(Color); break;
                        case Hatch.Checkers: pat = w.HatchCheckers(Color); break;
                        case Hatch.Cross: pat = w.HatchCross(Color); break;
                        case Hatch.DiagonalCross: pat = w.HatchDiagCross(Color); break;
                        case Hatch.Horizontal: pat = w.HatchHorizontal(Color); break;
                        case Hatch.Vertical: pat = w.HatchVertical(Color); break;
                        default: pat = w.HatchCross(Color); break;
                    }

                    w.CurrentPage.AddResource(pat);

                    contents.WriteLine("/Pattern cs");
                    contents.WriteLine("/" + pat.ResourceName + " scn");
                }
                else if (FillStyle == FillStyle.Pattern)
                {
                    if (Pattern != null)
                    {
                        var w = contents.writer;
                        PDFObjectPattern pat;


                        //make sure we have an image with resource reference
                        PDFObjectImage img = w.FindResource(Pattern) as PDFObjectImage;
                        if (img == null)
                        {
                            img = new PDFObjectImage(w, Pattern);
                            w.DefineResource(Pattern, img);
                        }


                        pat = PDFObjectPattern.FindOrCreatePattern(w, Pattern.Width, Pattern.Height,
                            PainterPDF.ITOA(Pattern.Width) + " 0 0 " + PainterPDF.ITOA(Pattern.Height) + " 0 0 cm", //set image transform matrix to fill cell
                            "/" + img.ResourceName + " Do"); //draw image

                        pat.AddResource(img); //image is a resource dependency in pattern
                        w.CurrentPage.AddResource(pat); //add resource if not already existing

                        
                        //setup pattern color space
                        contents.WriteLine("/Pattern cs");
                        contents.WriteLine("/" + pat.ResourceName + " scn");
                    }
                }
            }

        }

        public void UpdateClip(PDFObjectStream contents)
        {
            if (ChangedClip)
            {
                ChangedClip = false;

                //TODO: A problem here is that clipping path cannot be enlarged in PDF (its only intersected with the current one). 
                //Maybe we in the future can do some kind of save/restore hack to reset clipping path?
                
                if (Clip == null) //reset clipping rectangle
                    contents.WriteLine(PainterPDF.ITOA(0) + " " + PainterPDF.ITOA(0) + " " + PainterPDF.ITOA(contents.writer.Width) + " " + PainterPDF.ITOA(contents.writer.Height) + " re");
                else //new clipping rectangle
                    contents.WriteLine(PainterPDF.ITOA(Clip.XMin) + " " + PainterPDF.ITOA(Clip.YMin) + " " + PainterPDF.ITOA(Clip.Width) + " " + PainterPDF.ITOA(Clip.Height) + " re");

                contents.WriteLine("W n");
            }
        }
    }



    public class PainterPDF : Painter
    {

        int width, height; //each pixel/unit is 1/72 inch ~=0.352777778... millimeters
        int nextobjextid = 1;   //id number of next object created for this document
        internal List<PDFObject> AllObjects = new List<PDFObject>(); //all objects currently existing in pdf
        double paperwidthmm, paperheightmm; //size of pdf paper in millimeters
        PDFObjectCatalog catalog; // pdf root object
        internal PDFObjectPage CurrentPage;
        PDFAttributeState AttribState = new PDFAttributeState(); //Cached attribute changes. Applied on demand in drawing functions.

        string filename = null; //if not null, this is where contents are written on dispose
        Stream stream = null; //if not null, this is where contents are written on dispose

        Dictionary<object, PDFObject> ResourceLUT = new Dictionary<object, PDFObject>(); //cache for resource to avoid duplicate images etc. in pdf file


        public PainterPDF(Stream stream, PaperSize papersize, bool landscape)
        {
            this.stream = stream;
            SetupDefaults(papersize, landscape);
        }

        public PainterPDF(string filename, PaperSize papersize, bool landscape)
        {
            this.filename = filename;
            SetupDefaults(papersize, landscape);
        }


        private void SetupDefaults(PaperSize papersize, bool landscape)
        {
            PaperSizeToMM(papersize, landscape, out paperwidthmm, out paperheightmm);
            width = GFXUtil.FloatToInt((paperwidthmm / 25.4) * 72);
            height = GFXUtil.FloatToInt((paperheightmm / 25.4) * 72);

            catalog = new PDFObjectCatalog(this);
            Flush();    //add first page
        }

        static void PaperSizeToMM(PaperSize psize, bool landscape, out double width, out double height)
        {
            switch (psize)
            {
                case PaperSize.A0: width = 841; height = 1189; break; //A0
                case PaperSize.A1: width = 594; height = 841; break; //A1
                case PaperSize.A2: width = 420; height = 594; break; //A2
                case PaperSize.A3: width = 297; height = 420; break; //A3
                case PaperSize.A4: width = 210; height = 297; break; //A4
                case PaperSize.A5: width = 148; height = 210; break; //A5
                case PaperSize.Letter: width = 216; height = 279; break; //Letter
                case PaperSize.Legal: width = 216; height = 356; break; //Legal
                default: width = 210; height = 294; break;    //default to a4
            }

            if (landscape)
                MathUtil.Swap(ref width, ref height);
        }




        #region UTILS

        internal int GetNewObjectID()
        {
            int res = nextobjextid;
            nextobjextid++;
            return res;
        }


        internal static string RealsToString(params double[] r)
        {
            if (r == null)
                return string.Empty;

            List<string> vals = new List<string>();
            foreach (double d in r)
                vals.Add(RTOS(d));

            return string.Join(" ", vals);
        }

        /// <summary>
        /// Quick access to current pages content stream.
        /// </summary>
        PDFObjectStream Contents
        {
            get
            {
                return CurrentPage.Contents;
            }
        }


        void InitStroke()
        {
            AttribState.UpdateClip(Contents);
            AttribState.UpdateStrokeColor(Contents);
            AttribState.UpdateLineWidth(Contents);
            AttribState.UpdateLineJoin(Contents);
            AttribState.UpdateEndCaps(Contents);
            AttribState.UpdateLineStyle(Contents);
        }

        void Stroke(bool closed)
        {

            if (closed)
                Contents.WriteLine("s");
            else
                Contents.WriteLine("S");
        }


        void InitFill()
        {
            AttribState.UpdateClip(Contents);
            AttribState.UpdateStrokeColor(Contents);
            AttribState.UpdateFillStyle(Contents);
        }

        void Fill()
        {
            if (FillMode == Guppy2.GFX.FillMode.EvenOdd)
                Contents.WriteLine("f*");
            else
                Contents.WriteLine("f");    //winding fill
        }


        internal PDFObject FindResource(object obj)
        {
            PDFObject res;
            if (ResourceLUT.TryGetValue(obj, out res))
                return res;
            return null;
        }


        internal void DefineResource(object obj, PDFObject pdfobj)
        {
            ResourceLUT[obj] = pdfobj;
        }


        internal PDFObjectPattern HatchForwardDiag(int color)
        {
            return PDFObjectPattern.FindOrCreatePattern(this, 12, 12, RGBString(color) + " RG 2 J", "0 6 m 6 12 l S", "6 0 m 12 6 l S");

        }

        internal PDFObjectPattern HatchBackwardDiag(int color)
        {
            return PDFObjectPattern.FindOrCreatePattern(this, 12, 12, RGBString(color) + " RG 2 J", "0 6 m 6 0 l S", "6 12 m 12 6 l S");
        }

        internal PDFObjectPattern HatchCheckers(int color)
        {
            return PDFObjectPattern.FindOrCreatePattern(this, 12, 12, RGBString(color) + " rg", "0 0 m 6 0 l 6 6 l 0 6 l f", "6 6 m 12 6 l 12 12 l 6 12 l f");

        }

        internal PDFObjectPattern HatchCross(int color)
        {
            return PDFObjectPattern.FindOrCreatePattern(this, 12, 12, RGBString(color) + " RG", "6 0 m 6 12 l S", "0 6 m 12 6 l S");

        }

        internal PDFObjectPattern HatchDiagCross(int color)
        {
            return PDFObjectPattern.FindOrCreatePattern(this, 12, 12, RGBString(color) + " RG", "0 0 m 12 12 l S", "0 12 m 12 0 l S");

        }

        internal PDFObjectPattern HatchHorizontal(int color)
        {
            return PDFObjectPattern.FindOrCreatePattern(this, 12, 12, RGBString(color) + " RG", "0 6 m 12 6 l S");

        }


        internal PDFObjectPattern HatchVertical(int color)
        {
            return PDFObjectPattern.FindOrCreatePattern(this, 12, 12, RGBString(color) + " RG", "6 0 m 6 12 l S");
        }

        


        internal static string RGBString(int col)
        {
            byte br, bg, bb;
            RGB.Decode(col, out br, out bg, out bb);
            double r = br / 255.0;
            double g = bg / 255.0;
            double b = bb / 255.0;
            return RTOS(r) + " " + RTOS(g) + " " + RTOS(b);
        }


        #endregion

        #region ATTRIBUTES

        public override string Info
        {
            get { return "PDF Painter"; }
        }


        public override int Color
        {
            get
            {
                return base.Color;
            }
            set
            {
                if (value != Color)
                {
                    AttribState.Color = value;
                    AttribState.ChangedColor = true;    //stroke color
                    AttribState.ChangedFillStyle = true;    //fill style color
                }

                base.Color = value;
            }
        }


        public override double LineWidth
        {
            get
            {
                return base.LineWidth;
            }
            set
            {
                if (value != LineWidth)
                {
                    AttribState.LineWidth = value;
                    AttribState.ChangedLineWidth = true;
                }

                base.LineWidth = value;
            }
        }

        public override EndCap EndCaps
        {
            get
            {
                return base.EndCaps;
            }
            set
            {
                if (value != EndCaps)
                {
                    AttribState.EndCap = value;
                    AttribState.ChangedEndCap = true;
                }
                base.EndCaps = value;
            }
        }

        public override LineJoin LineJoin
        {
            get
            {
                return base.LineJoin;
            }
            set
            {
                if (value != LineJoin)
                {
                    AttribState.LineJoin = value;
                    AttribState.ChangedLineJoin = true;
                }
                base.LineJoin = value;
            }
        }


        public override LineStyle LineStyle
        {
            get
            {
                return base.LineStyle;
            }
            set
            {
                if (LineStyle != value)
                {
                    AttribState.LineStyle = value;
                    AttribState.ChangedLineStyle = true;
                }

                base.LineStyle = value;
            }
        }


        public override double[] LineStyleDashes
        {
            get
            {
                return base.LineStyleDashes;
            }
            set
            {
                if (LineStyleDashes != value)
                {
                    AttribState.LineStyleDashes = value;
                    AttribState.ChangedLineStyle = true;
                }

                base.LineStyleDashes = value;
            }
        }


        public override FillStyle FillStyle
        {
            get
            {
                return base.FillStyle;
            }
            set
            {
                if (FillStyle != value)
                {
                    AttribState.FillStyle = value;
                    AttribState.ChangedFillStyle = true;
                }
                base.FillStyle = value;
            }
        }


        public override Hatch Hatch
        {
            get
            {
                return base.Hatch;
            }
            set
            {
                if (value != Hatch)
                {
                    AttribState.Hatch = value;
                    AttribState.ChangedFillStyle = true;

                }
                base.Hatch = value;
            }
        }


        public override Picture Pattern
        {
            get
            {
                return base.Pattern;
            }
            set
            {
                if (value != Pattern && value != null)
                {
                    AttribState.Pattern = value;
                    AttribState.ChangedPattern = true;
                }
                
                base.Pattern = value;
            }
        }

        public override Rectangle2i Clip
        {
            get
            {
                return base.Clip;
            }
            set
            {
                if (value != Clip)
                {

                    AttribState.Clip = value;
                    AttribState.ChangedClip = true;
                }
                base.Clip = value;
            }
        }

        #endregion



        #region LINE_PRIMITIVES


        public override void SetPixel(int x, int y, int rgb)
        {

            //simulate pixel with an 1x1 filled rectangle
            var oldfillstyle = FillStyle;
            var oldcolor = Color;

            Color = rgb;
            FillStyle = FillStyle.Solid;

            FillRectangle(x, y, x + 1, y + 1);

            Color = oldcolor;
            FillStyle = oldfillstyle;

        }

        public override int GetPixel(int x, int y)
        {
            return 0;   //not possible in PDF Painter
        }

        public override void DrawLine(double x1, double y1, double x2, double y2)
        {
            //Contents.WriteLine(DCSStr(x_mm) + " " + DCSStr(y_mm) + " m"); //moveto
            //Contents.WriteLine(DCSStr(x_mm) + " " + DCSStr(y_mm) + " l"); //lineto

            InitStroke();
            Contents.WriteLine(RTOS(x1) + " " + RTOS(y1) + " m"); //moveto
            Contents.WriteLine(RTOS(x2) + " " + RTOS(y2) + " l"); //lineto
            Stroke(false);
        }

        public override void DrawEllipticArc(double cx, double cy, double aradius, double bradius, double tilt, double startangle, double sweepangle)
        {

            InitStroke();

            List<double> xy = new List<double>();
            GeomUtil.EllipticArcToBeziers(cx, cy, aradius, bradius, tilt, startangle, sweepangle, xy);
            Point2d p = GeomUtil.EvalEllipseParam(cx, cy, aradius, bradius, tilt, GeomUtil.EllipseAngleToParam(startangle,aradius,bradius));
            
            GFXPath pth = new GFXPath();
            pth.MoveTo(p.X, p.Y);
            for (int i = 0; i < xy.Count; i += 6)
                pth.BezierTo(xy[i], xy[i + 1], xy[i + 2], xy[i + 3], xy[i + 4], xy[i + 5]);

            WritePath(pth,null);

            Stroke(Math.Abs(sweepangle)>=MathUtil.Deg360-MathUtil.Epsilon);

        }


        public override void DrawCircle(double cx, double cy, double radius)
        {
            GFXPath pth = new GFXPath();
            pth.MoveTo(cx + radius, cy);
            pth.ArcTo(cx - radius, cy, 1.0);
            pth.ArcTo(cx + radius, cy, 1.0);
            pth.CloseSubPath();
            DrawPath(pth);
        }


        public override void DrawCircleT(double cx, double cy, double radius)
        {
            GFXPath pth = new GFXPath();
            pth.MoveTo(cx + radius, cy);
            pth.ArcTo(cx - radius, cy, 1.0);
            pth.ArcTo(cx + radius, cy, 1.0);
            pth.CloseSubPath();
            DrawPathT(pth);
        }


        public override void DrawArc(double x1, double y1, double x2, double y2, double bulge)
        {
            GFXPath pth = new GFXPath();
            pth.MoveTo(x1, y1);
            pth.ArcTo(x2, y2, bulge);
            DrawPath(pth);
        }


        public override void DrawArcT(double x1, double y1, double x2, double y2, double bulge)
        {
            GFXPath pth = new GFXPath();
            pth.MoveTo(x1, y1);
            pth.ArcTo(x2, y2, bulge);
            DrawPathT(pth);
        }

        public override void DrawPolyLine(bool close, params double[] xy)
        {
            if (xy.Length < 2)
                return;

            InitStroke();

            for (int i = 0; i < xy.Length; i += 2)
            {
                double x = xy[i];
                double y = xy[i + 1];

                if (i == 0)
                    Contents.WriteLine(RTOS(x) + " " + RTOS(y) + " m"); //moveto
                else
                    Contents.WriteLine(RTOS(x) + " " + RTOS(y) + " l"); //lineto
            }


            Stroke(close);
        }


        internal static string RTOS(double v)
        {
            return Conv.FloatToStr(v,3,FloatFormat.Decimal); //never creates scientific notation which is mandatory here
        }

        internal static string ITOA(int v)
        {
            return ITOA(v);
        }

        private bool WritePath(GFXPath path, Transform2d tr)
        {

            if (path.PathPoints.Count < 1)
                return false;

            double penx = 0.0, peny = 0.0;

            if (tr == null) tr = Transform2d.Identity;


            double x1, y1, x2, y2, x3, y3;  //keeps transformat coordinates

            foreach (GFXPathMoveTo node in path.PathPoints)
            {
                if (node is GFXPathLineTo)
                {
                    tr.Apply(node.X, node.Y, out x1, out y1, true);
                    Contents.WriteLine(RTOS(x1) + " " + RTOS(y1) + " l");
                }
                else if (node is GFXPathBezierTo)
                {
                    GFXPathBezierTo bz = node as GFXPathBezierTo;

                    tr.Apply(bz.XC1, bz.YC1, out x1, out y1, true);
                    tr.Apply(bz.XC2, bz.YC2, out x2, out y2, true);
                    tr.Apply(bz.X, bz.Y, out x3, out y3, true);

                    Contents.WriteLine(
                        RTOS(x1) + " " + RTOS(y1) + " " +
                        RTOS(x2) + " " + RTOS(y2) + " " +
                        RTOS(x3) + " " + RTOS(y3) + " c");
                }
                else if (node is GFXPathArcTo)
                {
                    GFXPathArcTo arcnode = (GFXPathArcTo)node;
                    List<double> xybez = new List<double>();
                    GeomUtil.ArcToBeziers(penx, peny, arcnode.X, arcnode.Y, arcnode.Bulge, xybez);

                    if (xybez.Count == 2)
                    {
                        tr.Apply(xybez[0], xybez[1], out x1, out y1, true);
                        Contents.WriteLine(RTOS(x1) + " " + RTOS(y1) + " l");
                    }
                    else
                    {
                        for (int i = 0; i < xybez.Count; i += 6)
                        {

                            tr.Apply(xybez[i], xybez[i + 1], out x1, out y1, true);
                            tr.Apply(xybez[i + 2], xybez[i + 3], out x2, out y2, true);
                            tr.Apply(xybez[i + 4], xybez[i + 5], out x3, out y3, true);

                            Contents.WriteLine(
                                RTOS(x1) + " " + RTOS(y1) + " " +
                                RTOS(x2) + " " + RTOS(y2) + " " +
                                RTOS(x3) + " " + RTOS(y3) + " c");
                        }
                    }

                }
                else if (node is GFXPathCloseSubPath)
                {
                    Contents.WriteLine("h");
                }
                else if (node is GFXPathMoveTo)
                { //always happens since all nodes inherits this one
                    tr.Apply(node.X, node.Y, out x1, out y1, true);
                    Contents.WriteLine(RTOS(x1) + " " + RTOS(y1) + " m");
                }

                penx = node.X;
                peny = node.Y;
            }

            return true;
        }

        

        public override void DrawPath(GFXPath path)
        {
            InitStroke();
            if (!WritePath(path, null)) return;
            Stroke(false); //not auto closed, but uses the possibly pushed close nodes          
        }

        public override void DrawPathT(GFXPath path)
        {
            InitStroke();
            if (!WritePath(path, Transform)) return;
            Stroke(false);
        }


        #endregion LINE_PRIMITIVES


        #region FILLED_PRIMITVES

        public override void FillGradientRectangle(double x1, double y1, double x2, double y2, int y1color, int y2color)
        {
            //TODO: implement
        }

        public override void FillRectangle(double x1, double y1, double x2, double y2)
        {

            InitFill();

            GFXPath pth = new GFXPath();
            pth.MoveTo(x1, y1);
            pth.LineTo(x2, y1);
            pth.LineTo(x2, y2);
            pth.LineTo(x1, y2);

            WritePath(pth, null);
            Fill();
        }

        public override void FillRectangleT(double x1, double y1, double x2, double y2)
        {

            InitFill();

            GFXPath pth = new GFXPath();
            pth.MoveTo(x1, y1);
            pth.LineTo(x2, y1);
            pth.LineTo(x2, y2);
            pth.LineTo(x1, y2);

            WritePath(pth, Transform);
            Fill();            
        }

        public override void FillPath(GFXPath path)
        {
            InitFill();
            if (!WritePath(path, null)) return;
            Fill();
        }

        public override void FillPathT(GFXPath path)
        {
            InitFill();
            if (!WritePath(path, Transform)) return;
            Fill();
        }


        #endregion




        #region TEXT

       
        private void _DrawTextT(string txt, double x, double y, double size, double angle, TextAlign align,bool transform)
        {

            FillStyle oldfill = FillStyle;
            if (oldfill != FillStyle.Solid)
                FillStyle = FillStyle.Solid;


            //TODO: obey foreground color, not fill style
            GFXPath path=null;
            using (PainterGDIPlus pgdi = new PainterGDIPlus(true)) //TODO: work away GDI dependency
            {
                pgdi.Font = Font;
                path = pgdi.GetTextPath(txt, x, y, size, angle, align, Transform2d.Identity);
            }
            if (path == null)
                return;

            if (path == null)
                return;

            if (transform)
                FillPathT(path);
            else
                FillPath(path);

            if (oldfill != FillStyle.Solid)
                FillStyle = oldfill;
        }

        public override void DrawText(string txt, double x, double y, double size, double angle, TextAlign align)
        {

            _DrawTextT(txt, x, y, size, angle, align, false);
        }

        public override void DrawTextT(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            _DrawTextT(txt, x, y, size, angle, align, true);
        }

        public override void GetTextSize(string txt, double size, out double w, out double h)
        {
            //TODO: remove dependence on GDI+
            using (PainterGDIPlus pgdi = new PainterGDIPlus(true))
            {
                pgdi.GetTextSize(txt, size, out w, out h);
            }
        }

        public override GFXPath GetTextPath(string txt, double x, double y, double size, double angle, TextAlign align, Transform2d t)
        {
            //TODO: remove dependence on GDI+
            using (PainterGDIPlus pgdi = new PainterGDIPlus(true))
            {
                return pgdi.GetTextPath(txt, x, y, size, angle, align,t);
            }
        }

        public override double[] GetTextBounds(string txt, double x, double y, double size, double angle, TextAlign align)
        {
            //TODO: remove dependence on GDI+
            using (PainterGDIPlus pgdi = new PainterGDIPlus(true))
            {
                return pgdi.GetTextBounds(txt,x,y,size,angle,align);
            }
        }

        public override void GetFontDim(double size, out double linespace_pixels, out double ascent_pixels, out double descent_pixels,out bool filled)
        {
            //TODO: remove dependence on GDI+
            using (PainterGDIPlus pgdi = new PainterGDIPlus(true))
            {
                pgdi.GetFontDim(size,out linespace_pixels,out ascent_pixels,out descent_pixels,out filled);
            }
        }

        #endregion

        //done
        #region IMAGES


        private void InternalDrawImage(Picture pic, double x, double y, Transform2d tr)
        {
            PDFObjectImage img = FindResource(pic) as PDFObjectImage;
            if (img == null)
            { //this resource is non existing
                img = new PDFObjectImage(this, pic);
                DefineResource(pic, img);
            }

            CurrentPage.AddResource(img);

            Transform2d imat = new Transform2d(pic.Width, 0.0, 0.0, pic.Height, x, y);
            if (tr != null)
            {

                imat = imat * tr;
            }

            Contents.WriteLine("q  %Save");
            Contents.WriteLine(RTOS(imat.AX) + " " + RTOS(imat.AY) + " " + RTOS(imat.BX) + " " + RTOS(imat.BY) + " " + RTOS(imat.TX) + " " + RTOS(imat.TY) + " cm");
            Contents.WriteLine("/" + img.ResourceName + " Do");
            Contents.WriteLine("Q  %Restore");
        }

        public override void DrawPicture(Picture p, double x, double y)
        {
            InternalDrawImage(p, x, y, null);
        }

        public override void DrawPictureT(Picture p, double x, double y)
        {
            InternalDrawImage(p, x, y, Transform);
        }

        public override Picture CreatePictureFromSize(int width, int height)
        {
            return new SoftPicture(width, height);
        }

        public override Picture CreatePictureFromStream(System.IO.Stream str)
        {
            return SoftPicture.FromStream(str);
        }

        public override Picture CreatePictureFromSoftPicture(SoftPicture img)
        {
            return img; // done already since thi painter uses soft pictures
        }

        public override Picture MakeCompatiblePicture(Picture p)
        {
            if (p == null)
                return null;

            if (p is SoftPicture)
                return p as SoftPicture;
            else
                return p.ToSoftPicture();
        }

        public override void CopyPicture(ref Picture pic, int x1, int y1, int x2, int y2)
        {
            //this is not possible in pdf targte, but we create an empty image of correct size, which is better than nothing
            MathUtil.Sort(ref x1, ref x2);
            MathUtil.Sort(ref y1, ref y2);
            int wi = x2 - x1 + 1;
            int he = y2 - y1 + 1;
            pic = CreatePictureFromSize(wi, he);
        }


        public override void Blit(Picture p, int dst_x, int dst_y, int dst_w, int dst_h, int src_x, int src_y, int src_w, int src_h)
        {
            //TODO: implement
        }

        #endregion

        #region MISC

        public override void Clear(int rgb)
        {
            //Do nothing, clear is not supported for PDF painter
        }

        public override int Width
        {
            get { return width; }
        }

        public override int Height
        {
            get { return height; }
        }

        public override void MillimetersToPixels(double mmx, double mmy, out double pixx, out double pixy)
        {
            //pdf is always 72 dpi
            double f = (72.0 / 25.4);
            pixx = f * mmx;
            pixy = f * mmy;
        }

        public override void Dispose()
        {
            //write out contents to target on dispose
            if (filename != null)
                Write(filename);
            else if (stream != null)
                Write(stream);
        }

        public override void Flush()
        {

            CurrentPage = catalog.Pages.AddPage();

            //reset some state vars.
            ResetAttributes(); //pdf resets attributes on each new page, reset our settings...
            AttribState = new PDFAttributeState(); //...and recreate the attribute state to initial state            

        }


        public double PaperWidthMM
        {
            get { return paperwidthmm; }
        }

        public double PaperHeightMM
        {
            get
            {
                return paperheightmm;
            }
        }

        private void Write(Stream stream)
        {
            WriteLine(stream, "%PDF-1.3");
            byte[] nonascii = new byte[] { (byte)'%', (byte)'í', (byte)'ì', (byte)'¦', (byte)'\r', (byte)'\n' };
            stream.Write(nonascii, 0, nonascii.Length);
            //WriteHeader(stream);

            //Build object output
            List<byte> objects = new List<byte>();
            foreach (PDFObject obj in AllObjects)
            {
                obj.FileOffset = (int)stream.Position;
                List<byte> outp = obj.FinalOutput();
                stream.Write(outp.ToArray(), 0, outp.Count);

            }


            WriteCrossReferenceAndTrailer(stream);


        }

        private void Write(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            Write(fs);
            fs.Close();
        }

        /// <summary>
        /// Writes the cross reference table and trailer. Must be called after all pdf objects are created.
        /// </summary>
        private void WriteCrossReferenceAndTrailer(Stream stream)
        {
            int numobjects = AllObjects.Count() + 1;   //add one for the mandatory 0 object
            int startxref = (int)stream.Position;

            WriteLine(stream, "xref");
            WriteLine(stream, "0 " + ITOA(numobjects));
            WriteLine(stream, "0000000000 65535 f"); //must always exists (?)
            for (int l = 0; l < AllObjects.Count; l++)
            {
                string ofstr = AllObjects[l].FileOffset.ToString(CultureInfo.InvariantCulture).PadLeft(10, '0');
                WriteLine(stream, ofstr + " 00000 n");
            }

            WriteLine(stream, "trailer");
            WriteLine(stream, "<<");
            WriteLine(stream, "/Size " + ITOA(numobjects));
            WriteLine(stream, "/Root " + catalog.RefString);
            //WriteLine(stream,"/Info 1 0 R");
            WriteLine(stream, ">>");
            WriteLine(stream, "startxref");
            WriteLine(stream, ITOA(startxref));
            WriteLine(stream, "%%EOF");
        }

        private void WriteLine(Stream stream, string str)
        {
            byte[] ascii = ASCIIEncoding.ASCII.GetBytes(str);
            stream.Write(ascii, 0, ascii.Length);
            stream.WriteByte((byte)'\r');
            stream.WriteByte((byte)'\n');
        }

        #endregion

    }


    abstract internal class PDFObject
    {
        public int ID;
        public PainterPDF writer;
        public List<byte> data = new List<byte>();
        public int FileOffset = 0;  //set when written to file

        public PDFObject(PainterPDF writer)
        {
            this.ID = writer.GetNewObjectID();
            this.writer = writer;
            writer.AllObjects.Add(this);
        }



        protected byte[] StringToBytes(string str)
        {
            byte[] ascii = ASCIIEncoding.ASCII.GetBytes(str);
            return ascii;
        }

        public void WriteLine(string str)
        {
            Write(StringToBytes(str + "\r\n"));
        }

        public void Write(params byte[] rawdata)
        {
            data.AddRange(rawdata);
        }

        public string RefString
        {
            get
            {
                return PainterPDF.ITOA(ID) + " 0 R";
            }
        }

        protected void AppendLine(List<byte> res, string str)
        {
            res.AddRange(StringToBytes(str + "\r\n"));
        }

        public virtual List<byte> FinalOutput()
        {
            List<byte> res = new List<byte>();
            AppendLine(res, PainterPDF.ITOA(ID) + " 0 obj");
            res.AddRange(data);
            AppendLine(res, "endobj");
            return res;
        }
    }

    internal class PDFObjectCatalog : PDFObject
    {
        public PDFObjectPages Pages;
        public PDFObjectCatalog(PainterPDF writer)
            : base(writer)
        {
            Pages = new PDFObjectPages(writer);
        }


        public override List<byte> FinalOutput()
        {
            List<byte> res = new List<byte>();
            AppendLine(res, PainterPDF.ITOA(ID) + " 0 obj");
            AppendLine(res, "<<");
            AppendLine(res, "/Type /Catalog");
            AppendLine(res, "/Pages " + Pages.RefString);
            AppendLine(res, ">>");
            AppendLine(res, "endobj");

            return res;
        }
    }


    internal class PDFObjectPages : PDFObject
    {
        List<PDFObjectPage> pages = new List<PDFObjectPage>();

        public PDFObjectPages(PainterPDF writer)
            : base(writer)
        {


        }

        public PDFObjectPage AddPage()
        {
            PDFObjectPage page = new PDFObjectPage(writer, this);
            pages.Add(page);
            return page;
        }

        public override List<byte> FinalOutput()
        {
            List<byte> res = new List<byte>();
            AppendLine(res, PainterPDF.ITOA(ID) + " 0 obj");
            AppendLine(res, "<<");
            AppendLine(res, "/Type /Pages");
            List<string> pageids = new List<string>();
            foreach (var page in pages) pageids.Add(page.RefString);
            AppendLine(res, "/Kids [" + string.Join(" ", pageids.ToArray()) + "]");
            AppendLine(res, "/Count " + PainterPDF.ITOA(pages.Count));
            AppendLine(res, ">>");
            AppendLine(res, "endobj");
            return res;
        }
    }

    internal class PDFObjectPage : PDFObject
    {

        public PDFObjectStream Contents;
        public PDFObjectPages Pages;
        private PDFObjectResources Resources = null;

        public PDFObjectPage(PainterPDF writer, PDFObjectPages pages)
            : base(writer)
        {
            Contents = new PDFObjectStream(writer);
            this.Pages = pages;
            this.writer = writer; //need this to know paper size
        }

        public override List<byte> FinalOutput()
        {
            List<byte> res = new List<byte>();
            AppendLine(res, PainterPDF.ITOA(ID) + " 0 obj");
            AppendLine(res, "<<");
            AppendLine(res, "/Type /Page");
            AppendLine(res, "/Parent " + Pages.RefString);
            AppendLine(res, "/Contents " + Contents.RefString);

            if (Resources != null)
                AppendLine(res, "/Resources " + Resources.RefString);

            string pw = (PainterPDF.RTOS(writer.PaperWidthMM * (72.0 / 25.4)));
            string ph = (PainterPDF.RTOS(writer.PaperHeightMM * (72.0 / 25.4)));

            AppendLine(res, "/MediaBox [0 0 " + pw + " " + ph + "]");
            AppendLine(res, ">>");
            AppendLine(res, "endobj");

            return res;
        }

        public void AddResource(PDFObject resobj)
        {
            if (Resources == null)
                Resources = new PDFObjectResources(writer);
            Resources.AddResource(resobj);
        }
    }


    internal class PDFObjectResources : PDFObject
    {
        List<PDFObjectImage> Images = new List<PDFObjectImage>();
        List<PDFObjectPattern> Patterns = new List<PDFObjectPattern>();

        public static int uniquecnt = 0;

        public PDFObjectResources(PainterPDF writer)
            : base(writer)
        {

        }


        public override List<byte> FinalOutput()
        {
            List<byte> res = new List<byte>();
            AppendLine(res, PainterPDF.ITOA(ID) + " 0 obj");
            AppendLine(res, "<<");

            if (Images.Count > 0)
            {
                List<string> xids = new List<string>();
                foreach (var img in Images)
                    xids.Add("/" + img.ResourceName + " " + img.RefString);
                //Here we can add more ids to xobject name dictionary
                AppendLine(res, "/XObject <<" + string.Join(" ", xids.ToArray()) + " >>");
            }


            if (Patterns.Count > 0)
            {
                List<string> patids = new List<string>();
                foreach (var pat in Patterns)
                    patids.Add("/" + pat.ResourceName + " " + pat.RefString);
                AppendLine(res, "/Pattern <<" + string.Join(" ", patids.ToArray()) + " >>");
            }



            AppendLine(res, ">>");
            return res;
        }


        public void AddResource(PDFObject obj)
        {
            if (obj is PDFObjectImage)
            {
                if (!Images.Contains(obj as PDFObjectImage))
                    Images.Add(obj as PDFObjectImage);
            }
            else if (obj is PDFObjectPattern)
            {
                if (!Patterns.Contains(obj as PDFObjectPattern))
                {
                    Patterns.Add(obj as PDFObjectPattern);
                }
            }
            else
                throw new Exception("Resources of type " + obj.GetType().UnderlyingSystemType.ToString() + " not supported");
        }
    }

    internal class PDFObjectImage : PDFObject
    {
        //image bytes are stired in data array of base class
        int width, height;
        public readonly string ResourceName;

        private static int imageidcnt = 1;


        public PDFObjectImage(PainterPDF writer, Picture pic)
            : base(writer)
        {
            PictureToImageData(pic);
            width = pic.Width;
            height = pic.Height;
            ResourceName = "Image" + PainterPDF.ITOA(imageidcnt++);
        }

        private void PictureToImageData(Picture pic)
        {
            //TODO: can we use a more compressed format for images??


            SoftPicture softpic = pic.ToSoftPicture();

            StringBuilder sb = new StringBuilder();
            int lincnt = 0;

            int srcy = softpic.Height - 1;  //reverse y since we store 0,0 in lower left of soft picture, but PDF assumes this is top left

            for (int y = 0; y < softpic.Height; y++)
            {
                for (int x = 0; x < softpic.Width; x++)
                {
                    int col = softpic.GetPixel(x, srcy);
                    byte r, g, b;
                    RGB.Decode(col, out r, out g, out b);
                    
                    string rs = Convert.ToString((int)r, 16);
                    while (rs.Length < 2) rs = "0" + rs;
                    string gs = Convert.ToString((int)g, 16);
                    while (gs.Length < 2) gs = "0" + gs;
                    string bs = Convert.ToString((int)b, 16);
                    while (bs.Length < 2) bs = "0" + bs;

                    sb.Append(rs);
                    sb.Append(gs);
                    sb.Append(bs);

                    lincnt += 6;
                    if (lincnt >= 80)
                    {
                        lincnt = 0;
                        sb.Append('\n');
                    }
                }

                srcy--;
            }

            sb.Append('\n');

            string resstr = sb.ToString();
            data.Clear();
            foreach (char ch in resstr)
                data.Add((byte)ch);


        }

        public override List<byte> FinalOutput()
        {
            List<byte> res = new List<byte>();
            AppendLine(res, PainterPDF.ITOA(ID) + " 0 obj");
            AppendLine(res, "<<");
            AppendLine(res, "/Type /XObject");
            AppendLine(res, "/Subtype /Image");
            AppendLine(res, "/Width " + PainterPDF.ITOA(width));
            AppendLine(res, "/Height " + PainterPDF.ITOA(height));
            AppendLine(res, "/ColorSpace /DeviceRGB");
            AppendLine(res, "/BitsPerComponent 8");
            AppendLine(res, "/Filter /ASCIIHexDecode");
            AppendLine(res, "/Length " + data.Count);
            AppendLine(res, ">>");
            AppendLine(res, "stream");
            res.AddRange(data);
            AppendLine(res, "endstream");
            AppendLine(res, "endobj");


            //TODO: support alpha channel of image, using SMask soft mask of the image dictionary, if alpha channel present

            return res;
        }
    }

    internal class PDFObjectStream : PDFObject
    {
        PDFObjectLength lengthobject;

        public PDFObjectStream(PainterPDF writer)
            : base(writer)
        {
            lengthobject = new PDFObjectLength(writer, this);
        }

        public override List<byte> FinalOutput()
        {

            List<byte> res = new List<byte>();
            AppendLine(res, PainterPDF.ITOA(ID) + " 0 obj");
            AppendLine(res, "<<");
            AppendLine(res, "/Length " + lengthobject.RefString);
            AppendLine(res, ">>");
            AppendLine(res, "stream");
            res.AddRange(data);
            AppendLine(res, "endstream");
            AppendLine(res, "endobj");

            return res;

        }
    }

    internal class PDFObjectLength : PDFObject
    {
        PDFObject obj;
        public PDFObjectLength(PainterPDF writer, PDFObject obj)
            : base(writer)
        {
            this.obj = obj;
        }

        public override List<byte> FinalOutput()
        {
            List<byte> res = new List<byte>();
            AppendLine(res, PainterPDF.ITOA(ID) + " 0 obj");
            AppendLine(res, PainterPDF.ITOA(obj.data.Count));
            AppendLine(res, "endobj");
            return res;
        }
    }


    internal class PDFObjectPattern : PDFObject
    {
        PDFObjectLength lengthobject;
        PDFObjectResources resources;
        int width;
        int height;
        string[] pattern_code;

        public readonly String ResourceName;
        private static int patternidcnt = 1;





        public PDFObjectPattern(PainterPDF writer, int width, int height, params string[] patcode)
            : base(writer)
        {
            lengthobject = new PDFObjectLength(writer, this);
            resources = new PDFObjectResources(writer);

            ResourceName = "P" + PainterPDF.ITOA(patternidcnt++);

            this.width = width;
            this.height = height;
            this.pattern_code = patcode;
        }


        public void AddResource(PDFObject resource)
        {
            resources.AddResource(resource);
        }

        public static PDFObjectPattern FindOrCreatePattern(PainterPDF writer, int width, int height, params string[] patcode)
        {
            bool create = true;

            //check for an identical pattern
            foreach (PDFObject po in writer.AllObjects)
            {
                PDFObjectPattern pattern = po as PDFObjectPattern;
                if (pattern != null)
                {
                    if (pattern.width == width && pattern.height == height && pattern.pattern_code.Length == patcode.Length)
                    {

                        create = false;

                        for (int l = 0; l < patcode.Length; l++) //check if identical pattern definition code
                        {
                            if (patcode[l] != pattern.pattern_code[l])
                            {
                                create = true;
                                break; //not identical pattern definition code => create new pattern
                            }
                        }

                        if (create == false)
                            return pattern; //this pattern is identical, use it
                    }
                }
            }



            return new PDFObjectPattern(writer, width, height, patcode);
        }

        public override List<byte> FinalOutput()
        {
            List<byte> res = new List<byte>();
            AppendLine(res, PainterPDF.ITOA(ID) + " 0 obj");
            AppendLine(res, "<<");
            AppendLine(res, "/Type /Pattern");
            AppendLine(res, "/PatternType 1");
            AppendLine(res, "/PaintType 1");
            AppendLine(res, "TilingType 2");
            AppendLine(res, "/BBox [0 0 " + PainterPDF.ITOA(width) + " " + PainterPDF.ITOA(height) + "]");
            AppendLine(res, "/XStep " + PainterPDF.ITOA(width));
            AppendLine(res, "/YStep " + PainterPDF.ITOA(height));
            AppendLine(res, "/Resources " + resources.RefString);
            AppendLine(res, "/Matrix [1 0 0 1 0 0]");
            AppendLine(res, ">>");

            AppendLine(res, "stream");


            /*AppendLine(res,"0 0 m");
            AppendLine(res,"12 12 l S");
            AppendLine(res,"12 0 m");
            AppendLine(res,"0 12 l S");*/


            foreach (string lin in pattern_code)
                AppendLine(res, lin);



            AppendLine(res, "endstream");
            AppendLine(res, "endobj");

            return res;
        }

    }
}
