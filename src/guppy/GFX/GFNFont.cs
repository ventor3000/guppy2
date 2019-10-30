using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Guppy2.GFX;
using Guppy2.Calc.Geom2d;

namespace Guppy2.GFX

{
    /// <summary>
    /// Representation of a glyph in a general font
    /// </summary>
    internal class GFNGlyph
    {
        public GFNGlyph(GFXPath path, double dx)
        {
            this.Path = path;
            this.DX = dx;
        }

        public readonly double DX;
        public readonly GFXPath Path;
    }

    /// <summary>
    /// Representation of a general font (.gfn)
    /// </summary>
    internal class GFNFont
    {
        double capheight = 1.0;
        double linespacing = 1.5;
        double ascent = 1.1; //TODO: load ascent+descent
        double descent = 0.2;
        
        bool filled;    //true if filled font, false if stroked
        string remark = null;

        Dictionary<char, GFNGlyph> CharLUT = new Dictionary<char, GFNGlyph>();

        private GFNFont()
        {
            

        }

        public string Remark
        {
            get
            {
                return remark ?? "";
            }
        }

        public double CapHeight
        {
            get
            {
                return capheight;
            }
        }

        public bool Filled
        {
            get
            {
                return filled;
            }
        }


        /*public double Ascent
        {
            get { return ascent; }
        }

        public double Descent
        {
            get
            {
                return descent;
            }
        }*/

        public double LineSpacing
        {
            get
            {
                return linespacing;
            }
        }

        public static GFNFont FromFile(string filename)
        {

            return FromString(File.ReadAllText(filename));
        }

        public static GFNFont FromStream(Stream stream)
        {
            StreamReader sr = new StreamReader(stream);
            string c=sr.ReadToEnd();
            return FromString(c);
        }

        public static GFNFont FromString(string fntdef)
        {
            string[] lines=fntdef.Split('\n');
            int linenum=1;
            
            bool hascapheight=false;
            bool haslinespacing=false;
            bool hasstyle = false;
            bool hasascent = false;
            bool hasdescent = false;


            GFNFont res = new GFNFont();

            foreach (string _line in lines)
            {
                string line = _line.Trim();
                if (line=="") //skip empty line
                    continue;

                int eqpos = line.LastIndexOf('=');
                if (eqpos < 0)
                    throw new Exception("Invalid definition in font on line " + linenum.ToString());
                string key = line.Substring(0, eqpos);
                string val = line.Substring(eqpos + 1);
                

                key = key.Trim();
                val = val.Trim();

                if(key.Length==3)
                    key = key.Trim('\''); //allow for quoted characters to allow for space definition etc.

                
                switch(key) {
                    case "capheight":
                        hascapheight=true;
                        res.capheight=StrToFloat(val);
                        break;
                    case "linespacing":
                        haslinespacing=true;
                        res.linespacing=StrToFloat(val);
                        break;
                    case "style":
                        if (val == "stroke")
                            res.filled = false;
                        else if (val == "fill")
                            res.filled = true;
                        else
                            throw new Exception("Invalid style in font defintion, line "+linenum.ToString());
                        hasstyle = true;
                        break;
                    case "ascent":
                        res.ascent = StrToFloat(val);
                        hasascent = true;
                        break;
                    case "descent":
                        res.descent = StrToFloat(val);
                        hasdescent = true;
                        break;
                    case "remark":
                        if (string.IsNullOrEmpty(res.remark))
                            res.remark = val;
                        else
                            res.remark += "\n" + val;
                        break;
                    default:
                        if (key.Length != 1)
                            throw new Exception("Invalid definition in font file, key ='" + key + "' on line "+linenum.ToString());
                        try {
                            GFNGlyph glyph = ParseGlyph(val);
                            res.CharLUT[key[0]] = glyph;
                        }
                        catch(Exception exc) {
                            throw new Exception("Invalid font program on line "+linenum.ToString()+" : "+exc.Message);
                        }
                        break;
                }
            
                linenum++;
            }


            if (!hascapheight) throw new Exception("capheight specifier in font missing");
            if (!haslinespacing) throw new Exception("linespacing specifier in font missing");
            if (!hasstyle) throw new Exception("style specifier in font missing");
            if (!hasdescent) throw new Exception("descent specifier in font missing");
            if (!hasascent) throw new Exception("ascent specifier in font missing");

            return res;

        }

        private static GFNGlyph ParseGlyph(string val)
        {
            string[] atoms = val.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            Stack<double> stack = new Stack<double>();
            GFXPath path=new GFXPath();
            double num;
            double? dx=null;
            double x,y,bulge,xctl1,yctl1,xctl2,yctl2;
            


            foreach (string atom in atoms)
            {
                if (double.TryParse(atom, NumberStyles.Float, CultureInfo.InvariantCulture, out num))
                    stack.Push(num);
                else
                { //not a number, assume operator
                    switch (atom)
                    {
                        case "dx":
                            dx = stack.Pop();
                            break;
                        case "m":   //move to
                            y = stack.Pop();
                            x = stack.Pop();
                            path.MoveTo(x, y);
                            break;
                        case "l":   //line to
                            y = stack.Pop();
                            x = stack.Pop();
                            path.LineTo(x, y);
                            break;
                        case "a":   //arc to
                            bulge = stack.Pop();
                            y = stack.Pop();
                            x = stack.Pop();
                            path.ArcTo(x, y, bulge);
                            break;
                        case "c": //curve to
                            y = stack.Pop();
                            x = stack.Pop();
                            yctl2 = stack.Pop();
                            xctl2 = stack.Pop();
                            yctl1 = stack.Pop();
                            xctl1 = stack.Pop();
                            path.BezierTo(xctl1, yctl1, xctl2, yctl2, x, y);
                            break;
                        default:
                            throw new Exception("Invalid font program operator '" + atom + "'");
                    }
                }
            }

            if (dx == null)
                throw new Exception("Glyph missing dx operator");

            return new GFNGlyph(path, (double)dx);
        }


        private static double StrToFloat(string val)
        {
            return double.Parse(val, NumberStyles.Float, CultureInfo.InvariantCulture);
        }


        public double GetGlyphWidth(double size,char ch)
        {

            double scale = size / capheight;

            GFNGlyph glyph;
            if (CharLUT.TryGetValue(ch, out glyph))
                return glyph.DX * scale;
            else
                return capheight;
        }

        public GFXPath GetTextPath(string txt,Transform2d transform)
        {
            GFNGlyph glyph;
            


            if(txt==null) return null;


            GFXPath res = new GFXPath();

            foreach (char ch in txt)
            {
                if (CharLUT.TryGetValue(ch, out glyph))
                {
                    GFXPath glyphpath = glyph.Path.TransformCopy(transform);
                    res.AppendPath(glyphpath);
                    transform = Transform2d.Translate(glyph.DX, 0.0) * transform;    //move to next character
                }
            }

            return res;
        }

        public double GetCellHeight(double size)
        {
            //height is simply ascender+descender scaled for size
            return (ascent + descent) * (size / capheight);
        }


        public double GetAscent(double size)
        {
            return ascent * (size / capheight);
        }

        public double GetDescent(double size)
        {
            return descent * (size / capheight);
        }

        public double GetLineSpacing(double size)
        {
            return linespacing * (size / capheight);
        }


        public double DrawGlyphT(char ch, Painter painter)
        {
            GFNGlyph glyph;
            if (CharLUT.TryGetValue(ch, out glyph))
            {
                if (filled)
                    painter.FillPathT(glyph.Path);
                else
                    painter.DrawPathT(glyph.Path);

                return glyph.DX;
            }
            else //dummy character/draw substitute for non existing character
            {   
                if (filled)
                    painter.FillRectangleT(0, 0, capheight*0.8, capheight);
                else
                    painter.DrawRectangleT(0, 0, capheight*0.8, capheight);
            }

            return capheight;  //defult dx height???
        }

        public void DrawString(Painter painter, string txt, Transform2d xform)
        {

            /*double scale = txtsize / capheight;
            Transform2 tr = Transform2.Scale(scale)*Transform2.Translate(align_dx,align_dy)*Transform2.Rotate(angle)*Transform2.Translate(x, y);

            var oldt = painter.Transform;
            painter.Transform = tr * painter.Transform;*/

            foreach (char ch in txt)
            {
                double dx = DrawGlyphT(ch, painter);
                painter.Transform = Transform2d.Translate(dx, 0.0) * painter.Transform;
            }

            //painter.Transform = oldt;

        }

    }

}
