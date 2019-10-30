using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guppy2.GUI
{
    public class SizerTable:SizerBase
    {


        public static SizerTable Instance = new SizerTable();


        private static void GetNumRowsCols(Group group,out int rows,out int columns) {
            int numch = group.Children.Count;
            int wrapcnt = Math.Max(1, group.WrapCount); //protect from silly values less than 1

            if (numch <= 0)
            {
                rows = columns = 0;
                return;
            }
            if (group.Vertical)
            {
                columns = Math.Min(wrapcnt, numch); //in case we only have one column, number of rows are decided by number of controls
                rows = (int)(Math.Ceiling(numch / (double) columns) + 1e-6);
            }
            else
            { //horizontal layout
                rows = Math.Min(wrapcnt, numch); //in case we only have one row, number of columns are decided by number of controls
                columns = (int)(Math.Ceiling(numch / (double)rows)+1e-6);
            }
        }

        /// <summary>
        /// Gets a child from the table layout at a specified position.
        /// </summary>
        /// <param name="group">The parent group</param>
        /// <param name="x">X position of the child</param>
        /// <param name="y">Y position of the child</param>
        /// <param name="numrows">Number of rows in the group</param>
        /// <param name="numcols">Number of columns in the group</param>
        /// <returns>Child at the x,y position, or null if non existing</returns>
        private static Widget GetChild(Group group, int x, int y,int numrows,int numcols)
        {
            int index;

            if (group.Vertical)
                index = x + y * numcols;
            else
                index = y + x * numrows;

            if (index < 0 || index >= group.Children.Count)
                return null;
            
            return group.Children[index];
        }

        bool RowIsExpanding(Group group,int row, int numrows, int numcolumns,out int minheight)
        {
            minheight = 0;
            bool res = false;
            for (int l = 0; l < numcolumns; l++)
            {
                Widget w = GetChild(group, l, row, numrows, numcolumns);
                if (w != null)
                {
                    if (w.LayoutResult.ExpandY) res = true;
                    minheight = Math.Max(minheight, w.LayoutResult.MinSize.Height);
                }
            }

            return res;
        }
        
        bool ColumnIsExpanding(Group group, int col, int numrows, int numcolumns,out int minwidth)
        {
            minwidth = 0;
            bool res = false;
            for (int l = 0; l < numrows; l++)
            {
                Widget w = GetChild(group, col, l, numrows, numcolumns);
                if (w != null)
                {
                    if (w.LayoutResult.ExpandX) res = true;
                    minwidth = Math.Max(minwidth, w.LayoutResult.MinSize.Width);
                }
                
            }
            return res;
        }

        public override void CalcChildrenFinalPos(Group group, Size2i size)
        {
            int numrows, numcols;
            Rect2i area = GetChildLayoutArea(group, size);

            GetNumRowsCols(group,out numrows, out numcols);

            int[] minheights = new int[numrows];
            int[] minwidths = new int[numcols];
            bool[] expandx = new bool[numcols];
            bool[] expandy = new bool[numrows];
            int[] row_allocations = new int[numrows]; //final height of table rows
            int[] col_allocations = new int[numcols]; //final width of table columns
            //compute the space usable for expanding vertically and the minimum space each
            //row and column needs. this is filled in minwidths,minheights,expandx,expandy
            #region COMPUTE_EXPANSION_CONSTRAINTS
            int rowexpandspace = area.Height; //vertical space available for expanding after gap and minheights subtracted
            int colexpandspace = area.Width; //horizontal space available for expanding after gap and minwidths subtracted
            int num_expanding_cols = 0; //number of vertically expanding rows
            int num_expanding_rows = 0; //number of horizontally expanding columns
            rowexpandspace -= Math.Max((numrows - 1) * group.GapY, 0); //remove gap size
            colexpandspace -= Math.Max((numcols - 1) * group.GapX, 0); //remove gap size
            for (int row = 0; row < numrows; row++)
            {
                expandy[row] = RowIsExpanding(group, row, numrows, numcols, out minheights[row]);
                if (expandy[row])
                    num_expanding_rows++;
                rowexpandspace -= minheights[row];

            }

            for (int col = 0; col < numcols; col++)
            {
                expandx[col] = ColumnIsExpanding(group, col, numrows, numcols, out minwidths[col]);
                if (expandx[col])

                    num_expanding_cols++;
                colexpandspace -= minwidths[col];

            }
            #endregion


            //Now compute the actual width and height of each cell of the table
            //into the row_allocations[] and col_allocations[] arrays, distributing possible expansion
            int row_expand_count=0; //needed for distribution of space
            int col_expand_count=0; //needed for distribution of space
            for (int row = 0; row < numrows; row++)
            { 
                if (expandy[row])
                {
                    row_allocations[row] = minheights[row] + DistributSpace(row, num_expanding_rows, rowexpandspace);
                    row_expand_count++;
                }
                else
                    row_allocations[row] = minheights[row];
            }
            for (int col = 0; col < numcols; col++)
            {
                if (expandx[col])
                {
                    col_allocations[col] = minwidths[col] + DistributSpace(col, num_expanding_cols, colexpandspace);
                    col_expand_count++;
                }
                else
                    col_allocations[col] = minwidths[col];
                
            }

            
            
            //now, we have an allocation for each row and column, so now we can compute the final allocation
            //for each child
            int left=area.Left;
            row_expand_count = 0;
            col_expand_count = 0;
            for (int col = 0; col < numcols; col++)
            {
                int top = area.Top;
                for (int row = 0; row < numrows; row++)
                {
                    Widget w = GetChild(group, col, row, numrows, numcols);
                    if (w == null) continue;

                    //compute the childs size and position inside the allocation rectangle in the table:
                    Rect2i tablealloc = new Rect2i(left, top, col_allocations[col], row_allocations[row]);
                    
                    
                   

                    //w.LayoutResult.FinalPos = StickRectangle(tablealloc, w.LayoutResult.MinSize, FinalStick(w));
                    w.LayoutResult.FinalPos = AlignRectangle(tablealloc, w.LayoutResult.MinSize, FinalAlign(w),w.LayoutResult.ExpandX,w.LayoutResult.ExpandY);

                    top += row_allocations[row] + group.GapY;


             

                }
                left += col_allocations[col] + group.GapX;
            }
        }

        protected override Size2i CalcChildrenMinimumSize(Group group)
        {
            int numrows, numcols;
            GetNumRowsCols(group, out numrows, out numcols);

            int totalheight = 0;
            int totalwidth = 0;

            //sum of height of all rows:
            for (int row = 0; row < numrows; row++)
            {
                int rowheight = 0;
                for (int col = 0; col < numcols; col++)
                {
                    Widget w = GetChild(group, col, row, numrows, numcols);
                    if (w == null) continue;
                    rowheight = Math.Max(rowheight, w.LayoutResult.MinSize.Height);
                }
                totalheight += rowheight;
            }

            //sum of width of all columns:
            for (int col = 0; col < numcols; col++)
            {
                int colwidth = 0;
                for (int row = 0; row < numrows; row++)
                {
                    Widget w = GetChild(group, col, row, numrows, numcols);
                    if (w == null) continue;
                    colwidth = Math.Max(colwidth, w.LayoutResult.MinSize.Width);
                }
                totalwidth += colwidth;
            }

            //add size for gaps
            totalwidth += Math.Max((numcols - 1) * group.GapX, 0);
            totalheight += Math.Max((numrows - 1) * group.GapY, 0);
            
            return new Size2i(totalwidth, totalheight);
        }
    }
}
