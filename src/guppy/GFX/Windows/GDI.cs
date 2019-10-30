using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;

namespace Guppy2.GFX.Windows
{

    public enum TernaryRasterOperations : uint
    {
        /// <summary>dest = source</summary>
        SRCCOPY = 0x00CC0020,
        /// <summary>dest = source OR dest</summary>
        SRCPAINT = 0x00EE0086,
        /// <summary>dest = source AND dest</summary>
        SRCAND = 0x008800C6,
        /// <summary>dest = source XOR dest</summary>
        SRCINVERT = 0x00660046,
        /// <summary>dest = source AND (NOT dest)</summary>
        SRCERASE = 0x00440328,
        /// <summary>dest = (NOT source)</summary>
        NOTSRCCOPY = 0x00330008,
        /// <summary>dest = (NOT src) AND (NOT dest)</summary>
        NOTSRCERASE = 0x001100A6,
        /// <summary>dest = (source AND pattern)</summary>
        MERGECOPY = 0x00C000CA,
        /// <summary>dest = (NOT source) OR dest</summary>
        MERGEPAINT = 0x00BB0226,
        /// <summary>dest = pattern</summary>
        PATCOPY = 0x00F00021,
        /// <summary>dest = DPSnoo</summary>
        PATPAINT = 0x00FB0A09,
        /// <summary>dest = pattern XOR dest</summary>
        PATINVERT = 0x005A0049,
        /// <summary>dest = (NOT dest)</summary>
        DSTINVERT = 0x00550009,
        /// <summary>dest = BLACK</summary>
        BLACKNESS = 0x00000042,
        /// <summary>dest = WHITE</summary>
        WHITENESS = 0x00FF0062,

        //Non standard operations        
        /// <summary>dest = (pat & src) | (!pat & dst)</summary>
        COPYFG = 0x00CA0749,
        /// <summary>dest = (!pat & src) | (pat & dst) </summary>
        COPYBG = 0x00AC0744
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TEXTMETRIC
    {
        public int tmHeight;
        public int tmAscent;
        public int tmDescent;
        public int tmInternalLeading;
        public int tmExternalLeading;
        public int tmAveCharWidth;
        public int tmMaxCharWidth;
        public int tmWeight;
        public int tmOverhang;
        public int tmDigitizedAspectX;
        public int tmDigitizedAspectY;
        public byte tmFirstChar;    // for compatibility issues it must be byte instead of char (see the comment for further details above)
        public byte tmLastChar;    // for compatibility issues it must be byte instead of char (see the comment for further details above)
        public byte tmDefaultChar;    // for compatibility issues it must be byte instead of char (see the comment for further details above)
        public byte tmBreakChar;    // for compatibility issues it must be byte instead of char (see the comment for further details above)
        public byte tmItalic;
        public byte tmUnderlined;
        public byte tmStruckOut;
        public byte tmPitchAndFamily;
        public byte tmCharSet;
    }


    public enum PenStyle
    {
        PS_GEOMETRIC = 65536,
        PS_COSMETIC = 0,
        PS_ALTERNATE = 8,
        PS_SOLID = 0,
        PS_DASH = 1,
        PS_DOT = 2,
        PS_DASHDOT = 3,
        PS_DASHDOTDOT = 4,
        PS_NULL = 5,
        PS_USERSTYLE = 7,
        PS_INSIDEFRAME = 6,
        PS_ENDCAP_ROUND = 0,
        PS_ENDCAP_SQUARE = 256,
        PS_ENDCAP_FLAT = 512,
        PS_JOIN_BEVEL = 4096,
        PS_JOIN_MITER = 8192,
        PS_JOIN_ROUND = 0,
        PS_STYLE_MASK = 15,
        PS_ENDCAP_MASK = 3840,
        PS_TYPE_MASK = 983040,
    }

    public enum BinaryRasterOperations
    {
        R2_BLACK = 1,
        R2_NOTMERGEPEN = 2,
        R2_MASKNOTPEN = 3,
        R2_NOTCOPYPEN = 4,
        R2_MASKPENNOT = 5,
        R2_NOT = 6,
        R2_XORPEN = 7,
        R2_NOTMASKPEN = 8,
        R2_MASKPEN = 9,
        R2_NOTXORPEN = 10,
        R2_NOP = 11,
        R2_MERGENOTPEN = 12,
        R2_COPYPEN = 13,
        R2_MERGEPENNOT = 14,
        R2_MERGEPEN = 15,
        R2_WHITE = 16
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFO
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

  

    
    internal enum BrushStyle
    {
        BS_SOLID = 0,
        BS_HOLLOW = 1,
        BS_NULL = 1,
        BS_HATCHED = 2,
        BS_PATTERN = 3,
        BS_INDEXED = 4,
        BS_DIBPATTERN = 5,
        BS_DIBPATTERNPT = 6,
        BS_PATTERN8X8 = 7,
        BS_DIBPATTERN8X8 = 8,
        BS_MONOPATTERN = 9
    }

    

    internal enum BkMode
    {
        TRANSPARENT = 1,
        OPAQUE = 2
    }

    
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left_, int top_, int right_, int bottom_)
        {
            Left = left_;
            Top = top_;
            Right = right_;
            Bottom = bottom_;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static implicit operator System.Drawing.Point(POINT p)
        {
            return new System.Drawing.Point(p.X, p.Y);
        }

        public static implicit operator POINT(System.Drawing.Point p)
        {
            return new POINT(p.X, p.Y);
        }
    }


    //TODO: supress security attribute for invokes to improve speed

    public class GDI
    {
        //        #define DIB_PAL_COLORS	1
        //          #define DIB_RGB_COLORS	0
        [DllImport("gdi32.dll")]
        [SuppressUnmanagedCodeSecurity]
        internal static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BITMAPINFO pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);


        [DllImport("gdi32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern int StretchDIBits(IntPtr hdc, int XDest, int YDest,
           int nDestWidth, int nDestHeight, int XSrc, int YSrc, int nSrcWidth,
           int nSrcHeight, byte[] lpBits, [In] ref BITMAPINFO lpBitsInfo, uint iUsage,
          TernaryRasterOperations dwRop);


        [DllImport("gdi32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern int StretchDIBits(IntPtr hdc, int XDest, int YDest,
           int nDestWidth, int nDestHeight, int XSrc, int YSrc, int nSrcWidth,
           int nSrcHeight, [In] int[] lpBits, [In] ref BITMAPINFO lpBitsInfo, uint iUsage,
          TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern int StretchDIBits(IntPtr hdc, int XDest, int YDest,
           int nDestWidth, int nDestHeight, int XSrc, int YSrc, int nSrcWidth,
           int nSrcHeight, IntPtr lpBits, [In] ref BITMAPINFO lpBitsInfo, uint iUsage,
          TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool Polyline(IntPtr hdc, [In] POINT[] lppt, int cPoints);

        [DllImport("gdi32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool Polygon(IntPtr hdc, [In] POINT[] lpPoints, int nCount);

        [DllImport("gdi32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern bool FillRgn(IntPtr hdc, IntPtr hrgn, IntPtr hbr);

        [DllImport("gdi32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        [SuppressUnmanagedCodeSecurity]
        internal static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        [SuppressUnmanagedCodeSecurity]
        internal static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        internal static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth,
            int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll")]
        internal static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
            int nWidthDest, int nHeightDest,
            IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
            TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll")]
        internal static extern uint SetPixel(IntPtr hdc, int X, int Y, int crColor);

        [DllImport("gdi32.dll")]
        internal static extern int GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreatePen(PenStyle fnPenStyle, int nWidth, int crColor);

        [DllImport("gdi32.dll")]
        internal static extern bool MoveToEx(IntPtr hdc, int X, int Y, IntPtr lpPoint);

        [DllImport("gdi32.dll")]
        internal static extern bool LineTo(IntPtr hdc, int nXEnd, int nYEnd);

        [DllImport("gdi32.dll")]
        internal static extern int SetROP2(IntPtr hdc, BinaryRasterOperations fnDrawMode);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateBrushIndirect([In] ref LOGBRUSH lplb);

        [DllImport("user32.dll")]
        internal static extern int FillRect(IntPtr hDC, [In] ref RECT lprc, IntPtr hbr);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr ExtCreatePen(PenStyle dwPenStyle, uint dwWidth,
            [In] ref LOGBRUSH lplb, uint dwStyleCount, uint[] lpStyle);

        [DllImport("gdi32.dll")]
        internal static extern int IntersectClipRect(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll")]
        internal static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);

        [DllImport("gdi32.dll")]
        internal static extern bool PlgBlt(IntPtr hdcDest, ref POINT lpPoints, IntPtr hdcSrc,
           int nXSrc, int nYSrc, int nWidth, int nHeight, IntPtr hbmMask, int xMask,
           int yMask);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        internal static extern bool Rectangle(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll")]
        internal static extern bool PatBlt(IntPtr hdc, int nXLeft, int nYLeft, int nWidth, int nHeight, TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll")]
        internal static extern uint SetBkColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll")]
        internal static extern int SetBkMode(IntPtr hdc, BkMode iBkMode);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreatePatternBrush(IntPtr hbmp);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, [In]byte[] lpvBits);

        [DllImport("gdi32.dll")]
        internal static extern uint SetTextColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr CreateSolidBrush(uint crColor);

        [DllImport("gdi32.dll")]
        internal static extern bool GetBrushOrgEx(IntPtr hdc, out POINT lppt);

        [DllImport("gdi32.dll")]
        internal static extern bool SetBrushOrgEx(IntPtr hdc, int nXOrg, int nYOrg, IntPtr lppt);

        [DllImport("gdi32.dll")]
        internal static extern bool Ellipse(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll")]
        public static extern bool Arc(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nXStartArc, int nYStartArc, int nXEndArc, int nYEndArc);

        [DllImport("gdi32.dll")]
        internal static extern int GetDeviceCaps(IntPtr hdc, DeviceCap nIndex);

        [DllImport("gdi32.dll")]
        internal static extern IntPtr GetCurrentObject(IntPtr hdc, ObjectType uObjectType);

        [DllImport("gdi32.dll")]
        internal static extern bool GetBitmapDimensionEx(IntPtr hBitmap, out SIZE lpDimension);

        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromDC(IntPtr hDC);

        [DllImport("user32.dll")]
        internal static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("gdi32.dll")]
        internal static extern int SetGraphicsMode(IntPtr hdc, GraphicsMode iMode);

        [DllImport("gdi32.dll")]
        internal static extern bool SetWorldTransform(IntPtr hdc, [In] ref XFORM lpXform);

        [DllImport("gdi32.dll",EntryPoint="GetObject")]
        internal static extern int GetBitmapObject(IntPtr hgdiobj, int cbBuffer, ref BITMAP lpvObject);

        [DllImport("gdi32.dll")]
        internal static extern int GetObject(IntPtr hgdiobj, int cbBuffer, IntPtr obj);

        [DllImport("gdi32.dll")]
        internal static extern bool SetViewportOrgEx(IntPtr hdc, int X, int Y, out POINT lpPoint);

        [DllImport("gdi32.dll")]
        public static extern bool GetTextMetrics(IntPtr hdc, out TEXTMETRIC lptm);

        #region STRUCTS

        /// <summary>
        ///   The XFORM structure specifies a world-space to page-space transformation.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct XFORM
        {
            public float eM11;
            public float eM12;
            public float eM21;
            public float eM22;
            public float eDx;
            public float eDy;

            public XFORM(float eM11, float eM12, float eM21, float eM22, float eDx, float eDy)
            {
                this.eM11 = eM11;
                this.eM12 = eM12;
                this.eM21 = eM21;
                this.eM22 = eM22;
                this.eDx = eDx;
                this.eDy = eDy;
            }

            /// <summary>
            ///   Allows implicit converstion to a managed transformation matrix.
            /// </summary>
            public static implicit operator System.Drawing.Drawing2D.Matrix(XFORM xf)
            {
                return new System.Drawing.Drawing2D.Matrix(xf.eM11, xf.eM12, xf.eM21, xf.eM22, xf.eDx, xf.eDy);
            }

            /// <summary>
            ///   Allows implicit converstion from a managed transformation matrix.
            /// </summary>
            public static implicit operator XFORM(System.Drawing.Drawing2D.Matrix m)
            {
                float[] elems = m.Elements;
                return new XFORM(elems[0], elems[1], elems[2], elems[3], elems[4], elems[5]);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public ushort bmPlanes;
            public ushort bmBitsPixel;
            public IntPtr bmBits;
        }


        #endregion


        #region ENUMS

        [StructLayout(LayoutKind.Sequential)]
        internal struct LOGBRUSH
        {
            public BrushStyle lbStyle;        //brush style
            public int lbColor;    //colorref RGB(...)
            public HatchStyle lbHatch;        //hatch style
        }


        internal enum HatchStyle
        {
            HS_HORIZONTAL = 0,       /* ----- */
            HS_VERTICAL = 1,       /* ||||| */
            HS_FDIAGONAL = 2,       /* \\\\\ */
            HS_BDIAGONAL = 3,       /* ///// */
            HS_CROSS = 4,       /* +++++ */
            HS_DIAGCROSS = 5       /* xxxxx */
        }

        internal enum DeviceCap
        {
            /// <summary>
            /// Device driver version
            /// </summary>
            DRIVERVERSION = 0,
            /// <summary>
            /// Device classification
            /// </summary>
            TECHNOLOGY = 2,
            /// <summary>
            /// Horizontal size in millimeters
            /// </summary>
            HORZSIZE = 4,
            /// <summary>
            /// Vertical size in millimeters
            /// </summary>
            VERTSIZE = 6,
            /// <summary>
            /// Horizontal width in pixels
            /// </summary>
            HORZRES = 8,
            /// <summary>
            /// Vertical height in pixels
            /// </summary>
            VERTRES = 10,
            /// <summary>
            /// Number of bits per pixel
            /// </summary>
            BITSPIXEL = 12,
            /// <summary>
            /// Number of planes
            /// </summary>
            PLANES = 14,
            /// <summary>
            /// Number of brushes the device has
            /// </summary>
            NUMBRUSHES = 16,
            /// <summary>
            /// Number of pens the device has
            /// </summary>
            NUMPENS = 18,
            /// <summary>
            /// Number of markers the device has
            /// </summary>
            NUMMARKERS = 20,
            /// <summary>
            /// Number of fonts the device has
            /// </summary>
            NUMFONTS = 22,
            /// <summary>
            /// Number of colors the device supports
            /// </summary>
            NUMCOLORS = 24,
            /// <summary>
            /// Size required for device descriptor
            /// </summary>
            PDEVICESIZE = 26,
            /// <summary>
            /// Curve capabilities
            /// </summary>
            CURVECAPS = 28,
            /// <summary>
            /// Line capabilities
            /// </summary>
            LINECAPS = 30,
            /// <summary>
            /// Polygonal capabilities
            /// </summary>
            POLYGONALCAPS = 32,
            /// <summary>
            /// Text capabilities
            /// </summary>
            TEXTCAPS = 34,
            /// <summary>
            /// Clipping capabilities
            /// </summary>
            CLIPCAPS = 36,
            /// <summary>
            /// Bitblt capabilities
            /// </summary>
            RASTERCAPS = 38,
            /// <summary>
            /// Length of the X leg
            /// </summary>
            ASPECTX = 40,
            /// <summary>
            /// Length of the Y leg
            /// </summary>
            ASPECTY = 42,
            /// <summary>
            /// Length of the hypotenuse
            /// </summary>
            ASPECTXY = 44,
            /// <summary>
            /// Shading and Blending caps
            /// </summary>
            SHADEBLENDCAPS = 45,

            /// <summary>
            /// Logical pixels inch in X
            /// </summary>
            LOGPIXELSX = 88,
            /// <summary>
            /// Logical pixels inch in Y
            /// </summary>
            LOGPIXELSY = 90,

            /// <summary>
            /// Number of entries in physical palette
            /// </summary>
            SIZEPALETTE = 104,
            /// <summary>
            /// Number of reserved entries in palette
            /// </summary>
            NUMRESERVED = 106,
            /// <summary>
            /// Actual color resolution
            /// </summary>
            COLORRES = 108,

            // Printing related DeviceCaps. These replace the appropriate Escapes
            /// <summary>
            /// Physical Width in device units
            /// </summary>
            PHYSICALWIDTH = 110,
            /// <summary>
            /// Physical Height in device units
            /// </summary>
            PHYSICALHEIGHT = 111,
            /// <summary>
            /// Physical Printable Area x margin
            /// </summary>
            PHYSICALOFFSETX = 112,
            /// <summary>
            /// Physical Printable Area y margin
            /// </summary>
            PHYSICALOFFSETY = 113,
            /// <summary>
            /// Scaling factor x
            /// </summary>
            SCALINGFACTORX = 114,
            /// <summary>
            /// Scaling factor y
            /// </summary>
            SCALINGFACTORY = 115,

            /// <summary>
            /// Current vertical refresh rate of the display device (for displays only) in Hz
            /// </summary>
            VREFRESH = 116,
            /// <summary>
            /// Horizontal width of entire desktop in pixels
            /// </summary>
            DESKTOPVERTRES = 117,
            /// <summary>
            /// Vertical height of entire desktop in pixels
            /// </summary>
            DESKTOPHORZRES = 118,
            /// <summary>
            /// Preferred blt alignment
            /// </summary>
            BLTALIGNMENT = 119
        }

        internal enum ObjectType
        {
            OBJ_PEN = 1,
            OBJ_BRUSH = 2,
            OBJ_DC = 3,
            OBJ_METADC = 4,
            OBJ_PAL = 5,
            OBJ_FONT = 6,
            OBJ_BITMAP = 7,
            OBJ_REGION = 8,
            OBJ_METAFILE = 9,
            OBJ_MEMDC = 10,
            OBJ_EXTPEN = 11,
            OBJ_ENHMETADC = 12,
            OBJ_ENHMETAFILE = 13
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;

            public SIZE(int cx, int cy)
            {
                this.cx = cx;
                this.cy = cy;
            }
        }

        /// <summary>
        ///   The graphics mode that can be set by SetGraphicsMode.
        /// </summary>
        public enum GraphicsMode : int
        {
            /// <summary>
            ///   Sets the graphics mode that is compatible with 16-bit Windows. This is the default mode. If
            ///   this value is specified, the application can only modify the world-to-device transform by
            ///   calling functions that set window and viewport extents and origins, but not by using
            ///   SetWorldTransform or ModifyWorldTransform; calls to those functions will fail.
            ///   Examples of functions that set window and viewport extents and origins are SetViewportExtEx
            ///   and SetWindowExtEx.
            /// </summary>
            GM_COMPATIBLE = 1,
            /// <summary>
            ///   Sets the advanced graphics mode that allows world transformations. This value must be
            ///   specified if the application will set or modify the world transformation for the specified
            ///   device context. In this mode all graphics, including text output, fully conform to the
            ///   world-to-device transformation specified in the device context.
            /// </summary>
            GM_ADVANCED = 2,
        }

        #endregion


    }
}
