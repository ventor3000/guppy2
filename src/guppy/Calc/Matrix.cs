using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guppy2.Calc
{
    public class Matrix
    {
        private double[][] m;


        public Matrix(int rows, int cols, params double[] elems)
        {
            m = new double[rows][];
            for (int l = 0; l < rows; l++)
                m[l] = new double[cols];

            int n = elems.Length;
            if (n == 0) return; //no elements given, return zero array
            if (n != cols * rows)
                throw new Exception("Wrong number of elements in Matrix constructor, must be 0 or columns*rows");

            int idx = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    m[row][col] = elems[idx++];
                }
            }
        }

        public static Matrix FromIdentity(int size)
        {

            Matrix result = new Matrix(size, size);
            
            for(int i=0;i<size;i++)
                result[i,i]=1.0;
            return result;
        }

        public double this[int row, int col]
        {
            get
            {
                return m[row][col];
            }
            set
            {
                m[row][col] = value;
            }
        }


        public int NumRows {get {return m.Length;}}

        public int NumColumns {get {return m[0].Length;}}

        public bool IsSquare {get { return NumRows == NumColumns; }}

        public bool IsSameSize(Matrix m) { return NumColumns == m.NumColumns && NumRows == m.NumRows; }
        

        public override string ToString()
        {

            double[] d=new double[NumColumns];
            List<String> rowstr = new List<string>();

            foreach(double[] row in m) {
                rowstr.Add("[" + StringUtil.FormatReals(row) + "]");
            }

            return "[" + string.Join(",", rowstr) + "]";
        }


        /// <summary>
        /// Returns a matrix with row,col removed
        /// </summary>
        public Matrix GetMinor(int row, int col)
        {

            int nr=NumRows,nc=NumColumns;
            Matrix result = new Matrix(nr - 1, nc - 1);
            int dstrow = 0, dstcol = 0;

            for (int r = 0; r < nr; r++)
            {
                if (r != row)
                {
                    for (int c = 0; c < nc; c++)
                    {
                        if (c != col)
                        {
                            result[dstrow, dstcol] = m[r][c];
                            dstcol++;
                        }
                    }

                    dstcol = 0;
                    dstrow++;
                }
            }

            return result;
        }


        

        #region OPERATORS

        public static Matrix operator +(Matrix m1, Matrix m2)
        {

            if (!m1.IsSameSize(m2))
                throw new Exception("Matrix operator+ => can only be applied to matrices of equal size");

            int nr=m1.NumRows,nc=m1.NumColumns;
            Matrix result = new Matrix(nr,nc);

            for (int col = 0; col < nc; col++)
            {
                for (int row = 0; row < nr; row++)
                {
                    result[row, col] = m1[row, col] + m2[row, col];
                }
            }
            
            return result;
        }

        public static Matrix operator -(Matrix m1, Matrix m2)
        {

            if (!m1.IsSameSize(m2))
                throw new Exception("Matrix operator- => can only be applied to matrices of equal size");

            int nr = m1.NumRows, nc = m1.NumColumns;
            Matrix result = new Matrix(nr, nc);

            for (int col = 0; col < nc; col++)
            {
                for (int row = 0; row < nr; row++)
                {
                    result[row, col] = m1[row, col] - m2[row, col];
                }
            }

            return result;
        }

        public static Matrix operator*(Matrix m1,Matrix m2) {
            int num=m1.NumColumns;
            if(num !=m2.NumRows)
                throw new Exception("Matrix operator* => wrong number of rows/columns to multiply");

            Matrix result=new Matrix(m1.NumRows,m2.NumColumns);

            for (int col = 0; col < result.NumColumns; col++)
            {
                for (int row = 0; row < result.NumRows; row++)
                {
                    for (int i = 0; i < num; i++)
                        result[row, col] += m1[row, i] * m2[i, col];
                }
            }

            return result;

            /*
             *  if (matrix1.ColumnCount != matrix2.RowCount)
                throw new ArithmeticException("Number of columns in first matrix does not equal number of rows in second matrix.");

            DoubleMatrix result = new DoubleMatrix(matrix2.ColumnCount, matrix1.RowCount);

            for (int j = 0; j < result.RowCount; j++)
                for (int i = 0; i < result.ColumnCount; i++)
                {
                    Double value = 0;
                    for (int k = 0; k < matrix1.ColumnCount; k++)
                        value += matrix1[k, j] * matrix2[i, k];
                    result[i, j] = value;
                }

            return result;*/
        }

        public double[] GetRow(int rowidx)
        {
            return m[rowidx];
        }


        public Matrix Transposed
        {
            get
            {
                int nr=NumRows;
                int nc=NumColumns;
                Matrix res = new Matrix(nc, nr);
                for (int i = 0; i < nc; i++)
                {
                    for (int j = 0; j < nr; j++)
                    {
                        res[i, j] = this[j, i];
                    }
                }
                return res;
            }
        }

        public static Matrix operator *(Matrix m, double v)
        {
            int nr=m.NumRows;
            int nc=m.NumColumns;
            Matrix res = new Matrix(nr,nc);
            for (int r = 0; r < nr; r++)
            {
                double[] srcrow=m.m[r];
                double[] dstrow = res.m[r];
                for (int c = 0; c < nc; c++)
                {
                    dstrow[c] = srcrow[c] * v;
                }
            }

            return res;
        }

        public static Matrix operator *(double v,Matrix m)
        {
            return m * v;
        }
        
        #endregion OPERATORS
    }
}
