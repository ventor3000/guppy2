using Guppy2.GFX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2
{
    public class Size2i
    {
        public readonly int Width,Height;


        public static Size2i Natural = new Size2i(-1, -1);
        public static readonly Size2i Zero = new Size2i(0, 0);

        public Size2i(int w, int h) { this.Width = w; this.Height = h; }

        public override string ToString()
        {
            return Width.ToString() + "," + Height.ToString();
        }

        public Size2i Grow(Size2i s)
        {
            return new Size2i(Width+s.Width, Height+s.Height);
        }

        public Size2i Grow(Margin m) {
            return new Size2i(Width+m.Width,Height+m.Height);
         }

        

        public Size2i Shrink(Size2i s)
        {
            return new Size2i(Math.Max(0, Width - s.Width), Math.Max(0, Height - s.Height));
        }

        public Size2i Shrink(Margin s)
        {
            return new Size2i(Math.Max(0, Width - s.Width), Math.Max(0, Height - s.Height));
        }

        
        public static Size2i Max(Size2i s1,Size2i s2) {
            return new Size2i(Math.Max(s1.Width,s2.Width),Math.Max(s1.Height,s2.Height));
        }
    }

    public class Rect2i
    {
        public readonly int X,Y,Width,Height;

        public Rect2i(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        public Rect2i(Point2i pos, Size2i size)
        {
            X = pos.X;
            Y = pos.Y;
            Width = size.Width;
            Height = size.Height;
        }

        public override string ToString()
        {
            return X.ToString() + "," + Y.ToString() + "," + Width.ToString() + "," + Height.ToString();
        }

        public Size2i Size { get { return new Size2i(Width, Height); } }

        public Rect2i Grow(Margin margin)
        {
            return new Rect2i(X - margin.Left, Y - margin.Top, Width + margin.Width, Height+margin.Height);
        }

        public Rect2i Shrink(Margin margin)
        {
            return new Rect2i(X + margin.Left, Y + margin.Top, Width - margin.Width, Height - margin.Height);
        }

        public Rect2i Translate(int dx, int dy)
        {
            return new Rect2i(X + dx, Y + dy, Width, Height);
        }
        

        public int Top { get { return Y; } }
        public int Bottom { get {return Y+Height;}}
        public int Left { get { return X; } }
        public int Right { get { return X + Width; } }
    }

    /*public class Point2i
    {
        public static Point2i Origin = new Point2i(0, 0);
        
        public readonly int X, Y;

        public Point2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return X.ToString() + "," + Y.ToString();
        }
    }*/

    public class Margin
    {
        public readonly int Left, Right, Top, Bottom;

        public static Margin None = new Margin(0, 0, 0, 0);

        public Margin(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Bottom = bottom;
            this.Right = right;
        }

        public Margin(int m)
        {
            this.Left =
            this.Right =
            this.Top =
            this.Bottom = m;
        }
                

        public int Width { get { return Left + Right; } }
        public int Height { get { return Top + Bottom; } }

        public override string ToString()
        {
            return Left.ToString() + "," + Top.ToString() + "," + Right.ToString() + "," + Bottom.ToString();
        }

    }
}
