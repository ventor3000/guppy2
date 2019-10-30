using Guppy2;
using Guppy2.AppUtils;
using Guppy2.Calc.Geom2d;
using Guppy2.GFX;
using Guppy2.GUI;
using Guppy2.GUI.ExtraWidgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2Test
{
    public class WinMain:Window
    {
        Canvas drawarea;    // this is where we draw current drawing
        Tabs drawingtabs;   // tab control for switching between drawings
        Label coordlabel;   // label showing coordinate info
        ProgressBar progbar; //our global progress bar
        internal Label StatusLabel;

        
        Point2i cursorpixel = Point2i.Origin;   //pixel position of cursor in viewport, y axis bottom up
        Point2d trackingpos = Point2d.Origo;    //cursor position in world coordiantes, or null if no drawing
        

        Picture backbuffer=null;
        Picture overlay=null;
        bool overlay_needs_background = false;  //if the background needs to be blitted on the overlay before overlay is drawn
        Rectangle2i screen_dirty = Rectangle2i.Empty;  //if not null, this is the part of the frontbuffer that contains temp graphics
        bool redraw_backbuffer = true; //if true, backbuffer is redrawin on next painting event

        InputCore basesink = new InputCore();
        InputCore eventsink = null; //if null, messages are sent to base sink

        public WinMain()
            : base("ToyCAD", WindowStyle.FrameMainWindow)
        {


           // testimage = Guppy.CreatePicture("c:\\temp\\green.png",PictureMode.Hardware);

            var toolbar = new Panel(this) { Vertical = false ,ExpandY=false};
            new Separator(toolbar, true);
            drawarea=new Canvas(this) { ExpandX = true, ExpandY = true};
            drawarea.EvRedraw += OnDrawAreaRedraw;
            drawarea.EvResized += OnDrawAreaResized;
            drawarea.EvMotion += OnDrawAreaMotion;
            drawarea.EvWheel += OnDrawAreaWheel;
            drawarea.EvButton += OnDrawAreaButton;

            drawingtabs = new Tabs(this) { ExpandX = true,TabSide=TabSide.Bottom };
            drawingtabs.EvChanged += OnDrawingTabsChanged;

            /*new TabPage(drawingtabs, "Alfa");
            new TabPage(drawingtabs, "Beta");
            new TabPage(drawingtabs, "Gamma");*/

            CmdNewDrawing("First");

            var statuspanel = new Panel(this);
            StatusLabel = new Label(statuspanel, "ToyCAD (C) 2013-");

            var statusbar=new Panel(this) { Vertical = false };
            coordlabel=new Label(statusbar, " 100.00, 200.23") { Align=Guppy2.GUI.Align.Left,Width=300 };
            progbar=new ProgressBar(statusbar);

            

            CreateToolbarButton(toolbar, LTF._("New"), "NEW");
            CreateToolbarButton(toolbar, LTF._("Close"), "CLOSE");
            CreateToolbarButton(toolbar, LTF._("Open"), "OPEN");
            CreateToolbarButton(toolbar, LTF._("Save"), "SAVE");
            CreateToolbarButton(toolbar, LTF._("Line"), "LINE");
            CreateToolbarButton(toolbar, LTF._("Arc"), "ARC");
            CreateToolbarButton(toolbar, LTF._("Test"), "TEST");
        }

        void OnDrawAreaButton(object sender, ButtonEventArgs e)
        {
            if (e.Pressed && e.Status.HasFlag(KeyStatus.LeftButton))
            {
                CurrentSink.OnMouseDown(CursorWorldPos);
            }
        }

     

        internal InputCore CurrentSink
        { //gets the sink that will handle gui messages
            get
            {
                if (eventsink != null)
                    return eventsink;
                return basesink;
            }

            set
            {
                eventsink = value;
            }
        }

        void OnDrawAreaWheel(object sender, WheelEventArgs e)
        {
            Drawing drw = CurrentDrawing;
            if (drw != null)
            {
                double scale;
                if (e.Delta < 0)
                    scale = 0.8;
                else
                    scale = 1.0 / 0.8;

                drw.ViewTransform = drw.ViewTransform*Transform2d.Translate(-cursorpixel.X, -cursorpixel.Y) * Transform2d.Scale(scale) * Transform2d.Translate(cursorpixel.X, cursorpixel.Y);

                drawarea.Redraw();
            }
        }

        void OnDrawingTabsChanged(object sender, EventArgs e)
        {
            drawarea.RedrawLater();
        }

        void OnDrawAreaResized(object sender, EventArgs e)
        {
            drawarea.RedrawLater();
        }

        void OnDrawAreaMotion(object sender, MotionEventArgs e)
        {
            Drawing drw = CurrentDrawing;
            if (drw != null)
            {


                var size = drawarea.PhysicalSize;
                drawarea.Focused = true; //need focus for accepting wheel events

                var newcursorpixel = new Point2i(e.X, size.Height - e.Y - 1);
                double dx = newcursorpixel.X - cursorpixel.X;
                double dy = newcursorpixel.Y - cursorpixel.Y;
                cursorpixel = newcursorpixel;

                if (e.Status.HasFlag(KeyStatus.MiddleButton))
                {
                    drw.ViewTransform = drw.ViewTransform * Transform2d.Translate(dx, dy);
                    drawarea.Redraw();
                    return;
                }


                RedrawOverlay();
                
            }
            else
            {
                trackingpos = null;
            }
            

            

           // w.Caption = viewtransform.ToString(); // pt.ToString() + "    " + e.Status.ToString();

            //Caption = e.X.ToString();
            SetCoordText();
        }

        bool RedrawBackbuffer(Painter frontpainter)
        {

            Drawing drw = CurrentDrawing;

            if (drw == null) //no drawing active, just clear
            {
                frontpainter.Clear(RGB.Gray);
                return false;
            }

            //create backbuffer picture if to small or not existing
            Size2i siz=drawarea.PhysicalSize;
            if (backbuffer == null || backbuffer.Width < siz.Width || backbuffer.Height < siz.Height)
            {
                if (backbuffer != null)
                    backbuffer.Dispose();
                backbuffer = Guppy.CreatePicture(siz.Width, siz.Height,PictureMode.Software);
            }

            using (Painter backpainter = backbuffer.CreatePainter())
            {
                backpainter.Clear(0);
                drw.Draw(backpainter);
            }

            overlay_needs_background = true;

            return true;
        }

        Point2d CursorWorldPos
        {
            get
            {
                return new Point2d(cursorpixel.X, cursorpixel.Y).GetTransformed(CurrentDrawing.ViewTransform.Inversed);
            }
        }

        void RedrawOverlay()
        {
            Drawing drw = CurrentDrawing;
            if (drw == null || backbuffer==null)
                return; //no overlay if no drawing


            //Point2i dcs = cursorpixel;
            Point2d cursor_wcs = CursorWorldPos;

            //make sure overlay area is as big as backbuffer
            if (overlay == null || overlay.Width < backbuffer.Width || overlay.Height < backbuffer.Height)
            {
                if (overlay != null)
                    overlay.Dispose();
                overlay = Guppy.CreatePicture(backbuffer.Width, backbuffer.Height, PictureMode.Software);
            }


            using (PainterExtentsRecorder exrec = new PainterExtentsRecorder()) //to remember area drawn to
            {
                using (Painter overlaypainter = overlay.CreatePainter())
                {

                    if (overlay_needs_background)
                    {
                        overlaypainter.Blit(backbuffer, 0, 0, backbuffer.Width, backbuffer.Height, 0, 0, backbuffer.Width, backbuffer.Height);
                        overlay_needs_background = false;
                    }


                    using (PainterAggregate painter = new PainterAggregate(overlaypainter, exrec))
                    {
                        //let the current sink do its job
                        painter.Transform = CurrentDrawing.ViewTransform;
                        CurrentSink.OnTracking(cursorpixel, cursor_wcs,painter);
                    }


                    Rectangle2i fliprect = Rectangle2i.Empty; //the part of screen to flip
                    Rectangle2i newdirtyrect=fliprect; //the new dirty screen rectangle
                    
                    //compute slightly grown extents of area to show
                    var er = exrec.RecordedExtents;
                    if (!er.Empty) {
                        newdirtyrect=new Rectangle2i((int)(er.XMin - 5),(int)(er.YMin - 5),(int)(er.XMax + 5),(int)(er.YMax + 5));
                        fliprect=newdirtyrect;
                    }

                    //include old dirty area in flipping rectangle
                    if(!screen_dirty.IsEmpty) {
                        fliprect = fliprect.Append(screen_dirty.XMin, screen_dirty.YMin);
                        fliprect = fliprect.Append(screen_dirty.XMax, screen_dirty.YMax);
                    }

                    //flip if fliprect not empty.
                    if(!fliprect.IsEmpty) {
                        using (Painter frontpainter = drawarea.CreatePainter())
                        {
                            int x = fliprect.XMin, y = fliprect.YMin, w = fliprect.Width, h = fliprect.Height;
                            frontpainter.Blit(overlay, x, y, w, h, x, y, w, h);
                            overlaypainter.Blit(backbuffer, x, y, w, h, x, y, w, h);
                        }

                    }
                    

                    //remove the temp graphics from the overlay buffer
                    if (!screen_dirty.IsEmpty)
                    {
                        int x = screen_dirty.XMin, y = screen_dirty.YMin, w = screen_dirty.Width, h = screen_dirty.Height;
                        
                        overlaypainter.Color = RGB.Green;
                    }

                    screen_dirty = newdirtyrect;
                }
            }
        }

        void OnDrawAreaRedraw(object sender, RedrawEventArgs e)
        {


            if (redraw_backbuffer)
            {
                if (!RedrawBackbuffer(e.Painter))
                    return; //no active drawing
                screen_dirty = new Rectangle2i(0, 0, backbuffer.Width-1, backbuffer.Height-1 ); //entire screen is now dirty
            }

            RedrawOverlay();

            
        }

        private void CreateToolbarButton(Panel toolbar, string caption,string cmdid)
        {
            var p = new Button(toolbar, caption) { Flat = true ,Tip=caption,CanFocus=false,Tag=cmdid};
            p.EvClick+=(s,e) => {RunSystemCommand( ((Widget)s).Tag as string);};
        }

        void RunSystemCommand(string cmdid)
        {
            if (cmdid == "NEW")
                CmdNewDrawing("NONAME");
            else if (cmdid == "SAVE")
            {
                InputPoint ip = new InputPoint(null,null);
                ip.Run();
            }
            else if (cmdid == "LINE")
            {
                CommandLine.Run();
            }
            else if (cmdid == "ARC")
            {
                CommandArc.Run();
            }
            else if (cmdid == "TEST")
            {
                CommandTest.Run();
            }
            else
                Dialogs.Message("Error", "Unknown command: " + cmdid);
            // hypot:
        }

        void SetCoordText()
        {
            if (trackingpos == null)
                coordlabel.Caption = "---:---";
            else
                coordlabel.Caption = trackingpos.ToString();
        }

        void CmdNewDrawing(string name) {
            Drawing drw = new Drawing();
            TabPage tp = new TabPage(drawingtabs, name);
            tp.Tag = drw;
            drawingtabs.SelectedPage = tp;
            drawarea.RedrawLater();
        }

        

        internal Drawing CurrentDrawing
        {
            get
            {
                TabPage tp = drawingtabs.SelectedPage;
                if (tp == null)
                    return null;
                return tp.Tag as Drawing;
            }
        }

    }
}
