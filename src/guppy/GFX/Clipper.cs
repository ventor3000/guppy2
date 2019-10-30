using Guppy2.Calc;
using Guppy2.Calc.Geom2d;
using System;
using System.Collections.Generic;
using System.Text;


//Class for clipping stuff
namespace Guppy2.GFX
{
    public static class Clipper {

        private static IntPt[] clippts = new IntPt[10]; //used for temporary storage by some clipping algorithms
        
        private struct IntPt
        {
            public double X;
            public double Y;
            public bool Enter;  //if this is an entry point or an exit point in the clipping rectangle
        }

        private static double[] clipstore = new double[20]; //used for temporary storage

        
        private static int ClipLineEncode(int x, int y, int left, int top, int right, int bottom)
        {
            //helper function for 'ClipLine'
            int code = 0;
            if (x < left) code |= 1;
            if (x > right) code |= 2;
            if (y < top) code |= 8;
            if (y > bottom) code |= 4;
            return code;
        }

        public static bool ClipLine(ref int x1, ref int y1, ref int x2, ref int y2, int left, int top, int right, int bottom) {
            
            int code1, code2;
            bool draw = false;
            int swaptmp;
            float m; // slope

            // NOTE: this shitty function sometimes reverses the clipped output /RP

            while (true)
            {
                code1 = ClipLineEncode(x1, y1, left, top, right, bottom);
                code2 = ClipLineEncode(x2, y2, left, top, right, bottom);
                if ((code1 | code2) == 0)
                {
                    draw = true;
                    break;
                }
                else if ((code1 & code2) != 0)
                    break;
                else
                {
                    if (code1 == 0)
                    {
                        swaptmp = x2; x2 = x1; x1 = swaptmp;
                        swaptmp = y2; y2 = y1; y1 = swaptmp;
                        swaptmp = code2; code2 = code1; code1 = swaptmp;
                    }
                    if (x2 != x1)
                        m = (y2 - y1) / (float)(x2 - x1);
                    else
                        m = 1.0f;
                    if ((code1 & 1) != 0)
                    {
                        y1 += (int)((left - x1) * m);
                        x1 = left;
                    }
                    else if ((code1 & 2) != 0)
                    {
                        y1 += (int)((right - x1) * m);
                        x1 = right;
                    }
                    else if ((code1 & 4) != 0)
                    {
                        if (x2 != x1)
                            x1 += (int)((bottom - y1) / m);
                        y1 = bottom;
                    }
                    else if ((code1 & 8) != 0)
                    {
                        if (x2 != x1)
                            x1 += (int)((top - y1) / m);
                        y1 = top;
                    }
                }
            }
            return draw;
        }


        public static bool ClipLine(ref double x1, ref double y1, ref double x2, ref double y2, double clip_min_x, double clip_min_y, double clip_max_x, double clip_max_y)
        {
            // this function clips the sent line using the defined clipping
            // region using the cohen-sutherland clipping algorithm
            // returns false if the line is totally invisible, otherwise true

            // internal clipping codes
            const int CLIP_CODE_C = 0x0000;
            const int CLIP_CODE_N = 0x0008;
            const int CLIP_CODE_S = 0x0004;
            const int CLIP_CODE_E = 0x0002;
            const int CLIP_CODE_W = 0x0001;

            const int CLIP_CODE_NE = 0x000a;
            const int CLIP_CODE_SE = 0x0006;
            const int CLIP_CODE_NW = 0x0009;
            const int CLIP_CODE_SW = 0x0005;



            double xc1 = x1,
                   yc1 = y1,
                   xc2 = x2,
                   yc2 = y2;

            int p1_code = 0,
                p2_code = 0;

            /*int ClipMinY=0; //viewport.top-1;
            int ClipMaxY=viewport.bottom-viewport.top-1;
            int ClipMinX=0;
            int ClipMaxX=viewport.right-viewport.left-1;*/

            // determine codes for p1 and p2
            if (y1 < clip_min_y)
                p1_code |= CLIP_CODE_N;
            else
                if (y1 > clip_max_y)
                    p1_code |= CLIP_CODE_S;

            if (x1 < clip_min_x)
                p1_code |= CLIP_CODE_W;
            else
                if (x1 > clip_max_x)
                    p1_code |= CLIP_CODE_E;

            if (y2 < clip_min_y)
                p2_code |= CLIP_CODE_N;
            else
                if (y2 > clip_max_y)
                    p2_code |= CLIP_CODE_S;

            if (x2 < clip_min_x)
                p2_code |= CLIP_CODE_W;
            else
                if (x2 > clip_max_x)
                    p2_code |= CLIP_CODE_E;

            // try and trivially reject
            if ((p1_code & p2_code) != 0) //same side, must be totally invisible
                return false;

            // test for totally visible, if so leave points untouched
            if (p1_code == 0 && p2_code == 0) //both points inside must be fully visible
                return true;

            // determine end clip point for p1
            switch (p1_code)
            {
                case CLIP_CODE_C: break;

                case CLIP_CODE_N:
                    {
                        yc1 = clip_min_y;
                        xc1 = x1 + (clip_min_y - y1) * (x2 - x1) / (y2 - y1);
                    } break;
                case CLIP_CODE_S:
                    {
                        yc1 = clip_max_y;
                        xc1 = x1 + (clip_max_y - y1) * (x2 - x1) / (y2 - y1);
                    } break;

                case CLIP_CODE_W:
                    {
                        xc1 = clip_min_x;
                        yc1 = y1 + (clip_min_x - x1) * (y2 - y1) / (x2 - x1);
                    } break;

                case CLIP_CODE_E:
                    {
                        xc1 = clip_max_x;
                        yc1 = y1 + (clip_max_x - x1) * (y2 - y1) / (x2 - x1);
                    } break;

                // these cases are more complex, must compute 2 intersections
                case CLIP_CODE_NE:
                    {
                        // north hline intersection
                        yc1 = clip_min_y;
                        xc1 = x1 + (clip_min_y - y1) * (x2 - x1) / (y2 - y1);

                        // test if intersection is valid, of so then done, else compute next
                        if (xc1 < clip_min_x || xc1 > clip_max_x)
                        {
                            // east vline intersection
                            xc1 = clip_max_x;
                            yc1 = y1 + (clip_max_x - x1) * (y2 - y1) / (x2 - x1);
                        } // end if

                    } break;

                case CLIP_CODE_SE:
                    {
                        // south hline intersection
                        yc1 = clip_max_y;
                        xc1 = x1 + (clip_max_y - y1) * (x2 - x1) / (y2 - y1);

                        // test if intersection is valid, of so then done, else compute next
                        if (xc1 < clip_min_x || xc1 > clip_max_x)
                        {
                            // east vline intersection
                            xc1 = clip_max_x;
                            yc1 = y1 + (clip_max_x - x1) * (y2 - y1) / (x2 - x1);
                        } // end if

                    } break;

                case CLIP_CODE_NW:
                    {
                        // north hline intersection
                        yc1 = clip_min_y;
                        xc1 = x1 + (clip_min_y - y1) * (x2 - x1) / (y2 - y1);

                        // test if intersection is valid, of so then done, else compute next
                        if (xc1 < clip_min_x || xc1 > clip_max_x)
                        {
                            xc1 = clip_min_x;
                            yc1 = y1 + (clip_min_x - x1) * (y2 - y1) / (x2 - x1);
                        } // end if

                    } break;

                case CLIP_CODE_SW:
                    {
                        // south hline intersection
                        yc1 = clip_max_y;
                        xc1 = x1 + (clip_max_y - y1) * (x2 - x1) / (y2 - y1);

                        // test if intersection is valid, of so then done, else compute next
                        if (xc1 < clip_min_x || xc1 > clip_max_x)
                        {
                            xc1 = clip_min_x;
                            yc1 = y1 + (clip_min_x - x1) * (y2 - y1) / (x2 - x1);
                        } // end if

                    } break;

                default: break;

            } // end switch

            // determine clip point for p2
            switch (p2_code)
            {
                case CLIP_CODE_C: break;

                case CLIP_CODE_N:
                    {
                        yc2 = clip_min_y;
                        xc2 = x2 + (clip_min_y - y2) * (x1 - x2) / (y1 - y2);
                    } break;

                case CLIP_CODE_S:
                    {
                        yc2 = clip_max_y;
                        xc2 = x2 + (clip_max_y - y2) * (x1 - x2) / (y1 - y2);
                    } break;

                case CLIP_CODE_W:
                    {
                        xc2 = clip_min_x;
                        yc2 = y2 + (clip_min_x - x2) * (y1 - y2) / (x1 - x2);
                    } break;

                case CLIP_CODE_E:
                    {
                        xc2 = clip_max_x;
                        yc2 = y2 + (clip_max_x - x2) * (y1 - y2) / (x1 - x2);
                    } break;

                // these cases are more complex, must compute 2 intersections
                case CLIP_CODE_NE:
                    {
                        // north hline intersection
                        yc2 = clip_min_y;
                        xc2 = x2 + (clip_min_y - y2) * (x1 - x2) / (y1 - y2);

                        // test if intersection is valid, of so then done, else compute next
                        if (xc2 < clip_min_x || xc2 > clip_max_x)
                        {
                            // east vline intersection
                            xc2 = clip_max_x;
                            yc2 = y2 + (clip_max_x - x2) * (y1 - y2) / (x1 - x2);
                        } // end if

                    } break;

                case CLIP_CODE_SE:
                    {
                        // south hline intersection
                        yc2 = clip_max_y;
                        xc2 = x2 + (clip_max_y - y2) * (x1 - x2) / (y1 - y2);

                        // test if intersection is valid, of so then done, else compute next
                        if (xc2 < clip_min_x || xc2 > clip_max_x)
                        {
                            // east vline intersection
                            xc2 = clip_max_x;
                            yc2 = y2 + (clip_max_x - x2) * (y1 - y2) / (x1 - x2);
                        } // end if

                    } break;

                case CLIP_CODE_NW:
                    {
                        // north hline intersection
                        yc2 = clip_min_y;
                        xc2 = x2 + (clip_min_y - y2) * (x1 - x2) / (y1 - y2);

                        // test if intersection is valid, of so then done, else compute next
                        if (xc2 < clip_min_x || xc2 > clip_max_x)
                        {
                            xc2 = clip_min_x;
                            yc2 = y2 + (clip_min_x - x2) * (y1 - y2) / (x1 - x2);
                        } // end if

                    } break;

                case CLIP_CODE_SW:
                    {
                        // south hline intersection
                        yc2 = clip_max_y;
                        xc2 = x2 + (clip_max_y - y2) * (x1 - x2) / (y1 - y2);

                        // test if intersection is valid, of so then done, else compute next
                        if (xc2 < clip_min_x || xc2 > clip_max_x)
                        {
                            xc2 = clip_min_x;
                            yc2 = y2 + (clip_min_x - x2) * (y1 - y2) / (x1 - x2);
                        } // end if

                    } break;

                default: break;

            } // end switch

            // do bounds check
            if ((xc1 < clip_min_x) || (xc1 > clip_max_x) ||
              (yc1 < clip_min_y) || (yc1 > clip_max_y) ||
              (xc2 < clip_min_x) || (xc2 > clip_max_x) ||
              (yc2 < clip_min_y) || (yc2 > clip_max_y))
            {
                return false;
            } // end if

            // store vars back
            x1 = xc1;
            y1 = yc1;
            x2 = xc2;
            y2 = yc2;

            return true;
        }




        /// <summary>
        /// Clips a circle to a box, filling in an array with an (x1,y1,x2,y2,bulge) entry
        /// for each resulting arcs, returning the number of entries put in the result array (5*number of arcs).
        /// This function returns 1 if the entire circle is visible or 0 for totally invisible.
        /// </summary>
        public static int ClipCircle(double cx, double cy, double rad, double[] clipbuffer, double xmin, double ymin, double xmax, double ymax)
        {

            //circles bbox
            double cx1 = cx - rad, cy1 = cy - rad, cx2 = cx + rad, cy2 = cy + rad;

            //totally visible?
            if (cx1 >= xmin && cx2 <= xmax && cy1 >= ymin && cy2 <= ymax)
                return 1; //special case: draw entire circle
            

            //totally invisible?
            if (cx1 > xmax || cx2 < xmin || cy1 > ymax || cy2 < ymin)
                return 0;

            int bufferindex = 0;
            int numint=IntersectCircleBox(cx, cy, rad, xmin, ymin, xmax, ymax);
            
            if (numint < 1)
                return 0; //no intersections, not entirely visible means invisible

            int previdx = numint - 1;
            int idx = 0;

            //check each subarc for visibillity and add to result if so
            for (int l = 0; l < numint; l++)
            {
                double x1 = clippts[previdx].X;
                double y1 = clippts[previdx].Y;

                if (IsArcInBox(cx, cy, x1, y1, xmin, ymin, xmax, ymax))
                {
                    double x2 = clippts[idx].X;
                    double y2 = clippts[idx].Y;
                    double bulge = BulgeFromCenter(cx, cy, x1, y1, x2, y2);
                    clipbuffer[bufferindex++] = x1;
                    clipbuffer[bufferindex++] = y1;
                    clipbuffer[bufferindex++] = x2;
                    clipbuffer[bufferindex++] = y2;
                    clipbuffer[bufferindex++] = bulge;
                }

                previdx = idx;
                idx++;
            }


            return bufferindex;

        }
        


        

        /// <summary>
        /// Otherwise returns the number of entries filled into the clipbuffer array (can be 0 on entirely invisible)
        /// The number of arcs is return value/5 since each arc entry contains (x1,y1,x2,y2,bulge)
        /// </summary>
        public static int ClipArc(double x1, double y1, double x2, double y2, double bulge, double[] clipbuffer,double xmin, double ymin, double xmax, double ymax)
        {

            double cx, cy, r,subbulge;
            int bufferindex = 0;


            if (!GeomUtil.GetArcCenterFromBulge(x1, y1, x2, y2, bulge, out cx, out cy))
            {
                //arc is a line
                if (ClipLine(ref x1, ref y1, ref x2, ref y2, xmin, ymin, xmax, ymax))
                {
                    clipbuffer[0] = x1;
                    clipbuffer[1] = y1;
                    clipbuffer[2] = x2;
                    clipbuffer[3] = y2;
                    clipbuffer[4] = 0.0;
                    return 5;
                }
                return 0;
            }

            r=GeomUtil.GetArcRadiusFromBulge(x1,y1,x2,y2,bulge);


            //check if arcs circle is totally visible or invisible
            //circles bbox in cx1,cy1,cx2,cy2
            double cx1 = cx - r, cy1 = cy - r, cx2 = cx + r, cy2 = cy + r;

            //totally visible?
            if (cx1 >= xmin && cx2 <= xmax && cy1 >= ymin && cy2 <= ymax)
            {
                clipbuffer[0] = x1;
                clipbuffer[1] = y1;
                clipbuffer[2] = x2;
                clipbuffer[3] = y2;
                clipbuffer[4] = bulge;
                return 5;
            }
            
            //totally invisible?
            if (cx1 > xmax || cx2 < xmin || cy1 > ymax || cy2 < ymin)
                return 0;

            int numint=IntersectArcBox(x1,y1,x2,y2,bulge,cx, cy, r, xmin, ymin, xmax, ymax);

            if (numint == 0)
            { //no intersection, either totally visible or totally invisible
                if (RectangleContains(x1,y1, xmin, ymin, xmax, ymax))
                {
                    clipbuffer[0] = x1;
                    clipbuffer[1] = y1;
                    clipbuffer[2] = x2;
                    clipbuffer[3] = y2;
                    clipbuffer[4] = bulge;
                    return 5;
                }
                return 0;
            }
                

            //only works ccw and reverse result if needed
            bool reversed = false;
            if (bulge < 0.0)
            { 
                reversed = true;
                MathUtil.Swap(ref x1,ref x2);
                MathUtil.Swap(ref y1,ref y2);
                bulge= - bulge;
            }

            int previdx=numint-1;
            
            for (int i = 0; i < numint; i++)
            {
                double ix1=clippts[previdx].X,iy1=clippts[previdx].Y,ix2=clippts[i].X,iy2=clippts[i].Y;
                bool subclip = false;

                //if arcs start point is to right of line, modify end
                if (numint==1 || GeomUtil.SideOfPoint(ix1, iy1, ix2, iy2, x1, y1) < 0.0) 
                {
                    if (RectangleContains(x1, y1, xmin, ymin, xmax, ymax))
                    {
                        //if startpoint is inside clip, first part of arc must be included
                        subbulge = BulgeFromCenter(cx, cy, x1, y1, ix2, iy2);

                        if (reversed)
                        {
                            clipbuffer[bufferindex++] = ix2;
                            clipbuffer[bufferindex++] = iy2;
                            clipbuffer[bufferindex++] = x1;
                            clipbuffer[bufferindex++] = y1;
                            clipbuffer[bufferindex++] = -subbulge;
                        }
                        else
                        {
                            clipbuffer[bufferindex++] = x1;
                            clipbuffer[bufferindex++] = y1;
                            clipbuffer[bufferindex++] = ix2;
                            clipbuffer[bufferindex++] = iy2;
                            clipbuffer[bufferindex++] = subbulge;
                        }


                    }
                    subclip = true;
                }

                if (numint==1 || GeomUtil.SideOfPoint(ix1, iy1, ix2, iy2, x2, y2) < 0.0)
                {
                    if (RectangleContains(x2, y2, xmin, ymin, xmax, ymax))
                    {
                        //if endpoint is inside clip, last part of arc must be included
                        subbulge = BulgeFromCenter(cx, cy, ix1, iy1, x2, y2);

                        if (reversed)
                        {
                            clipbuffer[bufferindex++] = x2;
                            clipbuffer[bufferindex++] = y2;
                            clipbuffer[bufferindex++] = ix1;
                            clipbuffer[bufferindex++] = iy1;
                            clipbuffer[bufferindex++] = -subbulge;
                        }
                        else
                        {
                            clipbuffer[bufferindex++] = ix1;
                            clipbuffer[bufferindex++] = iy1;
                            clipbuffer[bufferindex++] = x2;
                            clipbuffer[bufferindex++] = y2;
                            clipbuffer[bufferindex++] = subbulge;
                        }

                       // p.Color = Colors.Green;
                       // p.DrawArc(ix1, iy1, x2, y2, subbulge);
                    }
                    subclip = true;
                }



                if (!subclip)
                { //this line is not subclipped by endpoints, consider entire segment
                    if (IsArcInBox(cx, cy, ix1, iy1,xmin,ymin,xmax,ymax))
                    {
                        subbulge = BulgeFromCenter(cx, cy, ix1, iy1, ix2, iy2);

                        if (reversed)
                        {
                            clipbuffer[bufferindex++] = ix2;
                            clipbuffer[bufferindex++] = iy2;
                            clipbuffer[bufferindex++] = ix1;
                            clipbuffer[bufferindex++] = iy1;
                            clipbuffer[bufferindex++] = -subbulge;
                        }
                        else
                        {
                            clipbuffer[bufferindex++] = ix1;
                            clipbuffer[bufferindex++] = iy1;
                            clipbuffer[bufferindex++] = ix2;
                            clipbuffer[bufferindex++] = iy2;
                            clipbuffer[bufferindex++] = subbulge;
                        }
                    }
                }
                previdx = i;
            }
            
            return bufferindex;
        }

        private static bool IsArcInBox(double cx, double cy, double startx, double starty,double xmin,double ymin,double xmax,double ymax)
        {
            //check if an arc starting exactly at the clipping box bounds is visible inside box,
            //using the starting slope of the arc. We can (should) check exactly agains floating point number
            //since the box coordiantes are directly assigned.
            if (startx == xmax)
                return starty - cy > 0.0;
            else if (startx == xmin)
                return starty - cy < 0.0;
            else if (starty == ymin)
                return -(startx-cx) < 0.0;
            else if (starty == ymax)
                return -(startx-cx) > 0.0;
            
            return false; //this should never,ever happen
        }


        private static double BulgeFromCenter(double cx, double cy, double x1, double y1, double x2, double y2)
        {
            // warning: only for ccw arcs
            double a1=GeomUtil.Angle(cx,cy,x1,y1);
            double a2=GeomUtil.Angle(cx,cy,x2,y2);
            if(a2<a1) a2+=MathUtil.Deg360;
            return GeomUtil.GetArcBulgeFromSweepAngle(a2-a1);
        }

        private static bool RectangleContains(double x, double y, double xmin, double ymin, double xmax, double ymax)
        {
            return x > xmin && x < xmax && y > ymin && y < ymax; //important to not use >= or <= here
        }
      
        
        private static int IntersectCircleBox(double cx,double cy,double r,double xmin,double ymin,double xmax,double ymax) {

            double dx, dy, r2, x, y;
            int clipindex = 0;

            r2 = r * r;
            
            // NOTE: we solve intersections in an ccw order around rectangle

            //solve intersections for ymin line
            dy = Math.Abs(ymin - cy);
            if (r > dy)
            {
                dx = Math.Sqrt(r2 - dy * dy);
                x = cx - dx;
                if (x <= xmax && x >= xmin)
                {
                    clippts[clipindex].X = x;
                    clippts[clipindex].Y = ymin;
                    clipindex++;
                }
                x = cx + dx;
                if (x <= xmax && x >= xmin)
                {
                    clippts[clipindex].X = x;
                    clippts[clipindex].Y = ymin;
                    clipindex++;
                }
            }

            //solve intersections for xmax line
            dx = Math.Abs(xmax - cx);
            if (r > dx)
            {
                dy = Math.Sqrt(r2 - dx * dx);
                y = cy - dy;
                if (y <= ymax && y >= ymin)
                {
                    clippts[clipindex].X = xmax;
                    clippts[clipindex].Y = y;
                    clipindex++;
                }
                y = cy + dy;
                if (y <= ymax && y >= ymin)
                {
                    clippts[clipindex].X = xmax;
                    clippts[clipindex].Y = y;
                    clipindex++;
                }
            }

            //solve intersections for ymax line
            dy = Math.Abs(ymax - cy);
            if (r > dy)
            {
                dx = Math.Sqrt(r2 - dy * dy);
                x = cx + dx;
                if (x <= xmax && x >= xmin)
                {
                    clippts[clipindex].X = x;
                    clippts[clipindex].Y = ymax;
                    clipindex++;
                }
                x = cx - dx;
                if (x <= xmax && x >= xmin)
                {
                    clippts[clipindex].X = x;
                    clippts[clipindex].Y = ymax;
                    clipindex++;
                }
            }
            
            //solve intersections for xmin line
            dx = Math.Abs(xmin - cx);
            if (r > dx)
            {
                dy = Math.Sqrt(r2 - dx * dx);
                y = cy + dy;
                if (y <= ymax && y >= ymin)
                {
                    clippts[clipindex].X = xmin;
                    clippts[clipindex].Y = y;
                    clipindex++;
                }
                y = cy - dy;
                if (y <= ymax && y >= ymin)
                {
                    clippts[clipindex].X = xmin;
                    clippts[clipindex].Y = y;
                    clipindex++;
                }
            }
            
            return clipindex;
        }


        private static int IntersectArcBox(double x1,double y1,double x2,double y2,double bulge,double cx, double cy, double r, double xmin, double ymin, double xmax, double ymax)
        {

            double dx, dy, r2, x, y,side;
            int clipindex = 0;

            r2 = r * r;

            //NOTE: we solve intersections in an ccw order around rectangle

            //solve intersections for ymin line
            dy = Math.Abs(ymin - cy);
            if (r > dy)
            {
                dx = Math.Sqrt(r2 - dy * dy);
                x = cx - dx;
                if (x <= xmax && x >= xmin)
                {
                    side = GeomUtil.SideOfPoint(x1, y1, x2, y2, x, ymin);
                    if (bulge * side <= 0.0)
                    {
                        clippts[clipindex].X = x;
                        clippts[clipindex].Y = ymin;
                        clipindex++;
                    }
                }
                x = cx + dx;
                if (x <= xmax && x >= xmin)
                {
                    side = GeomUtil.SideOfPoint(x1, y1, x2, y2, x, ymin);
                    if (bulge * side <= 0.0)
                    {

                        clippts[clipindex].X = x;
                        clippts[clipindex].Y = ymin;
                        clipindex++;
                    }
                }
            }

            //solve intersections for xmax line
            dx = Math.Abs(xmax - cx);
            if (r > dx)
            {
                dy = Math.Sqrt(r2 - dx * dx);
                y = cy - dy;
                if (y <= ymax && y >= ymin)
                {
                    side = GeomUtil.SideOfPoint(x1, y1, x2, y2, xmax, y);
                    if (bulge * side <= 0.0)
                    {
                        clippts[clipindex].X = xmax;
                        clippts[clipindex].Y = y;
                        clipindex++;
                    }
                }
                y = cy + dy;
                if (y <= ymax && y >= ymin)
                {
                    side = GeomUtil.SideOfPoint(x1, y1, x2, y2, xmax, y);
                    if (bulge * side <= 0.0)
                    {
                        clippts[clipindex].X = xmax;
                        clippts[clipindex].Y = y;
                        clipindex++;
                    }
                }
            }

            //solve intersections for ymax line
            dy = Math.Abs(ymax - cy);
            if (r > dy)
            {
                dx = Math.Sqrt(r2 - dy * dy);
                x = cx + dx;
                if (x <= xmax && x >= xmin)
                {
                    side = GeomUtil.SideOfPoint(x1, y1, x2, y2, x, ymax);
                    if (bulge * side <= 0.0)
                    {
                        clippts[clipindex].X = x;
                        clippts[clipindex].Y = ymax;
                        clipindex++;
                    }
                }
                x = cx - dx;
                if (x <= xmax && x >= xmin)
                {
                    side = GeomUtil.SideOfPoint(x1, y1, x2, y2, x, ymax);
                    if (bulge * side <= 0.0)
                    {
                        clippts[clipindex].X = x;
                        clippts[clipindex].Y = ymax;
                        clipindex++;
                    }
                }
            }

            //solve intersections for xmin line
            dx = Math.Abs(xmin - cx);
            if (r > dx)
            {
                dy = Math.Sqrt(r2 - dx * dx);
                y = cy + dy;
                if (y <= ymax && y >= ymin)
                {
                    side = GeomUtil.SideOfPoint(x1, y1, x2, y2, xmin, y);
                    if (bulge * side <= 0.0)
                    {
                        clippts[clipindex].X = xmin;
                        clippts[clipindex].Y = y;
                        clipindex++;
                    }
                }
                y = cy - dy;
                if (y <= ymax && y >= ymin)
                {
                    side = GeomUtil.SideOfPoint(x1, y1, x2, y2, xmin, y);
                    if (bulge * side <= 0.0)
                    {

                        clippts[clipindex].X = xmin;
                        clippts[clipindex].Y = y;
                        clipindex++;
                    }
                }
            }

            return clipindex;
        }


        private static int IntersectEllipseBox(double cx,double cy,double arad,double brad,double tilt, double xmin, double ymin, double xmax, double ymax)
        {
            //we think of ellipse as unit circle and transform x1,y1-x2,y2 box to a transformed box x1,y1,x2,y2x3,y3,x4,y4
            // we can then intersect thoose lines to get parameters on each side of the original box, that is intersection points

            Transform2d tounispace = Transform2d.Translate(-cx, -cy) * Transform2d.Rotate(-tilt) * Transform2d.Stretch(1.0 / arad, 1.0 / brad);

            //convert to 4-point box and transform to unit circle space
            double xb1 = xmin, yb1 = ymin;
            double xb2 = xmax, yb2 = ymin;
            double xb3 = xmax, yb3 = ymax;
            double xb4 = xmin, yb4 = ymax;
            double t0,t1;

            int clipindex = 0;

            tounispace.Apply(xb1, yb1, out xb1, out yb1, true);
            tounispace.Apply(xb2, yb2, out xb2, out yb2, true);
            tounispace.Apply(xb3, yb3, out xb3, out yb3, true);
            tounispace.Apply(xb4, yb4, out xb4, out yb4, true);

            //get parameters on lines and append to result
            if (IntersectLineUnitCircle(xb1, yb1, xb2, yb2, out t0, out t1))
            {
                AddClipParamIf01RangeHorizontal(ref clipindex, t0, xmin, xmax, ymin,false);
                AddClipParamIf01RangeHorizontal(ref clipindex, t1, xmin, xmax, ymin,true);
            }

            if (IntersectLineUnitCircle(xb2, yb2, xb3, yb3, out t0, out t1))
            {
                AddClipParamIf01RangeVertical(ref clipindex, t0, ymin, ymax, xmax,false);
                AddClipParamIf01RangeVertical(ref clipindex, t1, ymin, ymax, xmax,true);
            }

            if (IntersectLineUnitCircle(xb3, yb3, xb4, yb4, out t0, out t1))
            {
                AddClipParamIf01RangeHorizontal(ref clipindex, t0, xmax, xmin, ymax,false);
                AddClipParamIf01RangeHorizontal(ref clipindex, t1, xmax, xmin, ymax,true);
            }

            if (IntersectLineUnitCircle(xb4, yb4, xb1, yb1, out t0, out t1))
            {
                AddClipParamIf01RangeVertical(ref clipindex, t0, ymax, ymin, xmin, false);
                AddClipParamIf01RangeVertical(ref clipindex, t1, ymax, ymin, xmin, true);
            }

            return clipindex;
        }

        private static void AddClipParamIf01RangeHorizontal(ref int clipindex, double t, double xmin, double xmax, double y,bool entrypoint)
        {
            if (t >= 0.0 && t <= 1.0)
            {
                clippts[clipindex].X = xmin + (xmax - xmin) * t;
                clippts[clipindex].Y = y;
                clippts[clipindex].Enter = entrypoint;
                clipindex++;
            }
        }

        private static void AddClipParamIf01RangeVertical(ref int clipindex, double t, double ymin, double ymax, double x,bool entrypoint)
        {
            if (t >= 0.0 && t < 1.0)
            {
                clippts[clipindex].Y = ymin + (ymax - ymin) * t;
                clippts[clipindex].X = x;
                clippts[clipindex].Enter = entrypoint;
                clipindex++;
            }
        }



        private static bool IntersectLineUnitCircle(double x0, double y0, double x1, double y1, out double t0, out double t1)
        {
            double dx = x1 - x0;
            double dy = y1 - y0;
            
            double c2=dx*dx+dy*dy;
            double c1=2*(dy*y0+dx*x0);
            double c0=x0*x0+y0*y0-1;

            return MathUtil.SolveQuadratic(c2, c1, c0, out t0, out t1);
        }
      

        /// <summary>
        /// Clips a general ellipse to a box, filling in an array with (t1,t2,t1,t2...) start/end angles of ellipse.
        /// for each resulting elliptic arc, returning the number of entries put in the result array (2*number of arcs).
        /// The angles in the resulting array is larger than 0 and the following larger (possibly bigger than 2*pi)
        /// Returns 1 is whole ellipse is visible (0,2*pi is put in clipbuffer)
        /// Returns 0 on entire ellipse invisible
        /// Otherwise returns the number of entries put in the clipbuffer (which is 2*number of elliptic arcs computed)
        /// Note that the result is in CCW order.
        /// </summary>
        public static int ClipEllipse(double cx, double cy, double arad, double brad, double tilt, double[] clipbuffer, double xmin, double ymin, double xmax, double ymax)
        {
            int numint=IntersectEllipseBox(cx, cy, arad, brad, tilt, xmin, ymin, xmax, ymax);
            if (numint == 0)
            { //either totally visible or totally invisible
                if (!RectangleContains(cx, cy, xmin, ymin, xmax, ymax)) //if center point out of box it must be fully invisible
                    return 0;

                Point2d elp = GeomUtil.EvalEllipseParam(cx, cy, arad, brad, tilt, 0.0);
                if(RectangleContains(elp.X, elp.Y, xmin, ymin, xmax, ymax)) { //fully visible?
                    clipbuffer[0]=0.0;
                    clipbuffer[1]=MathUtil.Deg360;
                    return 1; 
                }
                return 0;   //not visible
            }

            if (arad < 0.0) arad = 0.0;
            if (brad < 0.0) brad = 0.0;

            int previdx = numint - 1;
            int idx = 0;
            int bufferindex = 0;
            double prevangle = 0.0,curangle=0.0;

            //check each subarc for visibillity and add to result if so
            for (int l = 0; l < numint; l++)
            {
                if (clippts[previdx].Enter) //also works for ellipses (?)
                {
                    double x1 = clippts[previdx].X;
                    double y1 = clippts[previdx].Y;
                    double x2 = clippts[idx].X;
                    double y2 = clippts[idx].Y;

                    //fill in the two angles, taking ellipses tilt angle into account, and make sure its
                    //larger than the previous angle
                    prevangle = GeomUtil.Angle(cx, cy, x1, y1) - tilt;
                    clipbuffer[bufferindex++] = prevangle;

                    curangle = GeomUtil.Angle(cx, cy, x2, y2) - tilt;
                    if (curangle < prevangle) curangle += MathUtil.Deg360; //increasing angles
                    clipbuffer[bufferindex++] = prevangle = curangle;
                }

                previdx = idx;
                idx++;
            }

            return bufferindex;
        }

        /// <summary>
        /// Computes intersection points of an elliptic arc to a box in ccw order.
        /// The input ellipse is interpretted as beeing ccw (that is sweepangle>0)
        /// </summary>
        /// <param name="cx">x center</param>
        /// <param name="cy">y center</param>
        /// <param name="arad">major radius</param>
        /// <param name="brad">minor radius</param>
        /// <param name="tilt">inclination of ellipse</param>
        /// <param name="elpx1">start point x</param>
        /// <param name="elpy1">start point y</param>
        /// <param name="elpx2">end point x</param>
        /// <param name="elpy2">end point y</param>
        /// <param name="xmin">clipbox left</param>
        /// <param name="ymin">clipbox bottom</param>
        /// <param name="xmax">clipbox top</param>
        /// <param name="ymax">clipbox right</param>
        /// <returns></returns>
        public static int IntersectEllipticArcBox(
            double cx,
            double cy, 
            double arad,
            double brad, 
            double tilt, 
            double elpx1,
            double elpy1,
            double elpx2,
            double elpy2, 
            double xmin, 
            double ymin,
            double xmax, 
            double ymax
            )
        {

            

            //we think of ellipse as unit circle and transform x1,y1-x2,y2 box to a transformed box x1,y1,x2,y2x3,y3,x4,y4
            // we can then intersect thoose lines to get parameters on each side of the original box, that is intersection points

            Transform2d tounispace = Transform2d.Translate(-cx, -cy) * Transform2d.Rotate(-tilt) * Transform2d.Stretch(1.0 / arad, 1.0 / brad);

            //convert to 4-point box and transform to unit circle space
            double xb1 = xmin, yb1 = ymin;
            double xb2 = xmax, yb2 = ymin;
            double xb3 = xmax, yb3 = ymax;
            double xb4 = xmin, yb4 = ymax;
            double t0, t1;

            int clipindex = 0;

            tounispace.Apply(xb1, yb1, out xb1, out yb1, true);
            tounispace.Apply(xb2, yb2, out xb2, out yb2, true);
            tounispace.Apply(xb3, yb3, out xb3, out yb3, true);
            tounispace.Apply(xb4, yb4, out xb4, out yb4, true);

          
            //get parameters on lines and append to result
            if (IntersectLineUnitCircle(xb1, yb1, xb2, yb2, out t0, out t1))
            {
                if(GeomUtil.SideOfPoint(elpx1,elpy1,elpx2,elpy2,xmin+t0*(xmax-xmin),ymin)<=0) //point on correct side of start-end chord
                    AddClipParamIf01RangeHorizontal(ref clipindex, t0, xmin, xmax, ymin, false);
                if (GeomUtil.SideOfPoint(elpx1, elpy1, elpx2, elpy2, xmin + t1 * (xmax - xmin), ymin)<=0) //point on correct side of start-end chord
                    AddClipParamIf01RangeHorizontal(ref clipindex, t1, xmin, xmax, ymin, true);
            }

            if (IntersectLineUnitCircle(xb2, yb2, xb3, yb3, out t0, out t1))
            {
                if (GeomUtil.SideOfPoint(elpx1, elpy1, elpx2, elpy2, xmax, ymin + t0 * (ymax - ymin)) <=0) //point on correct side of start-end chord
                    AddClipParamIf01RangeVertical(ref clipindex, t0, ymin, ymax, xmax, false);
                if (GeomUtil.SideOfPoint(elpx1, elpy1, elpx2, elpy2, xmax, ymin + t1 * (ymax - ymin)) <=0) //point on correct side of start-end chord
                    AddClipParamIf01RangeVertical(ref clipindex, t1, ymin, ymax, xmax, true);
            }

            if (IntersectLineUnitCircle(xb3, yb3, xb4, yb4, out t0, out t1))
            {
                if (GeomUtil.SideOfPoint(elpx1, elpy1, elpx2, elpy2, xmax + t0 * (xmin - xmax), ymax) <=0) //point on correct side of start-end chord
                    AddClipParamIf01RangeHorizontal(ref clipindex, t0, xmax, xmin, ymax, false);
                if (GeomUtil.SideOfPoint(elpx1, elpy1, elpx2, elpy2, xmax + t1 * (xmin - xmax), ymax) <=0) //point on correct side of start-end chord
                    AddClipParamIf01RangeHorizontal(ref clipindex, t1, xmax, xmin, ymax, true);
            }

            if (IntersectLineUnitCircle(xb4, yb4, xb1, yb1, out t0, out t1))
            {
                if (GeomUtil.SideOfPoint(elpx1, elpy1, elpx2, elpy2, xmin, ymax + t0 * (ymin - ymax)) <=0) //point on correct side of start-end chord
                    AddClipParamIf01RangeVertical(ref clipindex, t0, ymax, ymin, xmin, false);
                if (GeomUtil.SideOfPoint(elpx1, elpy1, elpx2, elpy2, xmin, ymax + t1 * (ymin - ymax)) <=0) //point on correct side of start-end chord
                    AddClipParamIf01RangeVertical(ref clipindex, t1, ymax, ymin, xmin, true);
            }


            /*
            
            for(int i=0;i<clipindex;i++) {


                if (clippts[i].Enter)
                    p.Color = Colors.Green;
                else
                    p.Color = Colors.Red;

                p.DrawMark(clippts[i].X,clippts[i].Y, MarkType.DiagonalCross, 5);
                p.Font="Arial";
                p.DrawText(i.ToString(), clippts[i].X, clippts[i].Y, 30, 0.0, TextAlign.BottomLeft);
            }*/

            return clipindex;
        }

        /// <summary>
        /// Clips a general elliptic arc to a box, filling in an array with (t1,t2,t1,t2...) start/end angles of ellipse.
        /// for each resulting elliptic arc, returning the number of entries put in the result array (2*number of arcs).
        /// The angles in the resulting array is larger than 0 and always increasing for ccw arcs and decrasing for cw arcs (for easy calculation of sweepangles for each arc)
        /// This function returns 0 for totally invisible, otherwise returns the number of entries
        /// put in the clipbuffer array (which is always at least 2 if any part of ellipse visible).
        /// Note that the result is in same order as input arcs direction.
        /// </summary>
        public static int ClipEllipticArc(double cx, double cy, double arad, double brad, double tilt, double startangle,double sweepangle, double[] clipbuffer, double xmin, double ymin, double xmax, double ymax)
        {

            bool reversed = false; //we always work with ccw ellipse arc. If input is not ccw, we reversi it and reverse bac result later
            if (sweepangle < 0.0)
            {
                reversed = true;
                startangle = MathUtil.NormalizeAngle(startangle + sweepangle);
                sweepangle = -sweepangle;
            }


            Point2d start = GeomUtil.EvalEllipseParam(cx, cy, arad, brad, tilt, GeomUtil.EllipseAngleToParam(startangle, arad, brad));
            Point2d end = GeomUtil.EvalEllipseParam(cx, cy, arad, brad, tilt, GeomUtil.EllipseAngleToParam(startangle+sweepangle, arad, brad));

            if (arad < 0.0) arad = 0.0;
            if (brad < 0.0) brad = 0.0;


            int numint = IntersectEllipticArcBox(cx, cy, arad, brad, tilt, start.X, start.Y, end.X, end.Y, xmin, ymin, xmax, ymax);
            if (numint == 0)
            { //either totally visible or totally invisible
                if (RectangleContains(start.X, start.Y, xmin, ymin, xmax, ymax))
                { //totally visible
                    clipbuffer[0] = startangle;
                    clipbuffer[1] = startangle + sweepangle;
                    return 2;
                }
                return 0;   //totally invisible
            }

            
            int previdx = numint - 1;
            
            int bufferindex = 0;
            double prevangle = 0.0, curangle = 0.0;

            for (int i = 0; i < numint; i++)
            {
                double ix1 = clippts[previdx].X, iy1 = clippts[previdx].Y, ix2 = clippts[i].X, iy2 = clippts[i].Y;
                bool subclip = false;

                //if arcs start point is to right of line, modify end
                if (numint == 1 || GeomUtil.SideOfPoint(ix1, iy1, ix2, iy2, start.X, start.Y) < 0.0)
                {
                    if (RectangleContains(start.X, start.Y, xmin, ymin, xmax, ymax))
                    {
                        prevangle = GeomUtil.Angle(cx, cy, start.X, start.Y) - tilt;
                        clipbuffer[bufferindex++] = prevangle;


                        curangle =GeomUtil.Angle(cx, cy, ix2, iy2) - tilt;
                        if (curangle < prevangle) curangle += MathUtil.Deg360; //maintain increasing order
                        clipbuffer[bufferindex++] = curangle;
                    }
                    subclip = true;
                }

                if (numint == 1 || GeomUtil.SideOfPoint(ix1, iy1, ix2, iy2, end.X, end.Y) < 0.0)
                {
                    if (RectangleContains(end.X, end.Y, xmin, ymin, xmax, ymax))
                    {
                        prevangle = GeomUtil.Angle(cx, cy, ix1, iy1) - tilt;
                        clipbuffer[bufferindex++] = prevangle;

                        curangle = GeomUtil.Angle(cx, cy, end.X, end.Y) - tilt;
                        if (curangle < prevangle) curangle += MathUtil.Deg360; //maintain increasing order
                        clipbuffer[bufferindex++] = curangle;
                    }
                    subclip = true;
                }



                if (!subclip)
                { //this line is not subclipped by endpoints, consider entire segment
                    if (clippts[previdx].Enter)
                    {
                        prevangle = GeomUtil.Angle(cx, cy, ix1, iy1) - tilt;
                        clipbuffer[bufferindex++] = prevangle;

                        curangle=GeomUtil.Angle(cx, cy, ix2, iy2) - tilt;
                        if (curangle < prevangle) curangle += MathUtil.Deg360; //maintain increasing order
                        clipbuffer[bufferindex++] = curangle;
                    }
                }
                previdx = i;
            }


            if(reversed) 
                Array.Reverse(clipbuffer,0,bufferindex);

            return bufferindex;
        }


        /// <summary>
        /// Clips a bezier curve to a given rectangle. Creates 2 parameters in the
        /// result buffer per bezier arc. Returns the number of entries put in the result array.
        /// Returns 0 if curve not visible at all
        /// </summary>
        public static int ClipBezier(double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3, double[] clipbuffer,double xmin,double ymin,double xmax,double ymax)
        {

            //check for fully vissible or fully invisible by checking all
            //control points beeing inside cliprect
            int visiflag = 0;
            if (RectangleContains(x0, y0,xmin,ymin,xmax,ymax)) visiflag |= 1;
            if (RectangleContains(x1, y1, xmin, ymin, xmax, ymax)) visiflag |= 2;
            if (RectangleContains(x2, y2, xmin, ymin, xmax, ymax)) visiflag |= 4;
            if (RectangleContains(x3, y3, xmin, ymin, xmax, ymax)) visiflag |= 8;

            if (visiflag == 15) { clipbuffer[0] = 0.0; clipbuffer[1] = 1.0; return 2; }  //fully visible

            
            //coefficients for intersecting bezier with vertical line x=0
            double cx3 = x3 - 3 * x2 + 3 * x1 - x0;
            double cx2 = 3 * x2 - 6 * x1 + 3 * x0;
            double cx1 = 3 * x1 - 3 * x0;
            double cx0 = x0;

            //coefficients for intersecting bezier with horizontal line line y=0
            double cy3 = y3 - 3 * y2 + 3 * y1 - y0;
            double cy2 = 3 * y2 - 6 * y1 + 3 * y0;
            double cy1 = 3 * y1 - 3 * y0;
            double cy0 = y0;



            int n=0;
            double[] ts;

            //clipstore[n++] = 0.0;   //startpoint
            clipstore[n++] = 1.0;   //endpoint

            ts=MathUtil.SolveCubic(cx3, cx2, cx1, cx0 - xmin); //t for bezier intersecting xmin
            if (ts != null)
                foreach (double t in ts)
                    if (t >= 0.0 && t <= 1.0)
                        clipstore[n++] = t;

            ts = MathUtil.SolveCubic(cx3, cx2, cx1, cx0 - xmax); //t for bezier intersecting xmax
            if (ts != null)
                foreach (double t in ts)
                    if (t >= 0.0 && t <= 1.0)
                        clipstore[n++] = t;

            ts = MathUtil.SolveCubic(cy3, cy2, cy1, cy0 - ymin); //t for bezier intersecting ymin
            if (ts != null)
                foreach (double t in ts)
                    if (t >= 0.0 && t <= 1.0)
                        clipstore[n++] = t;

            ts = MathUtil.SolveCubic(cy3, cy2, cy1, cy0 - ymax); //t for bezier intersecting ymax
            if (ts != null)
                foreach (double t in ts)
                    if (t >= 0.0 && t <= 1.0)
                        clipstore[n++] = t;

            if(n>1) //if only start end end point, no need to sort
                Array.Sort(clipstore, 0, n);

            

          
            
            //decide which pieces has their midpoints inside rectangle
            double prevt = 0.0, nextt,evx,evy;
            int residx = 0;
            for (int l = 0; l < n; l ++)
            {
                nextt = clipstore[l];
                GeomUtil.EvalBezier(x0, y0, x1, y1, x2, y2, x3, y3, (prevt + nextt) * 0.5, out evx, out evy);
                if (RectangleContains(evx, evy, xmin, ymin, xmax, ymax))
                {
                    clipbuffer[residx++] = prevt;
                    clipbuffer[residx++] = nextt;
                }

                prevt = nextt;
            }

            return residx;
        }

    }
}

