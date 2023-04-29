using MathNet.Numerics.LinearAlgebra;


namespace BalanceBackEnd
{
    public static class MatrixMultiplicator
    {
        public static double[,] MultiplyDenseMatrixes(double[,] A, double[,] B, bool isTransponse = false)
        {
            if (A.GetLength(0) != B.GetLength(1))
            {
                throw new ArgumentException("Операция умножения двух " +
                "матриц выполнима только в том случае, если число столбцов в первом сомножителе равно числу строк во втором");
            }

            Matrix<double> a = Matrix<double>.Build.DenseOfArray(A);
            Matrix<double> b = Matrix<double>.Build.DenseOfArray(B);
            Matrix<double> c = a * b;


            if (isTransponse)
            {
                return c.Transpose().ToArray();
            }
            else
            {
                return c.ToArray();
            }
        }

        public static double[,] MultiplySparseMatrixes(double[,] A, double[,] B, bool isTransponse = false)
        {
            if (A.GetLength(0) != B.GetLength(1))
            {
                throw new ArgumentException("Операция умножения двух " +
                "матриц выполнима только в том случае, если число столбцов в первом сомножителе равно числу строк во втором");
            }

            Matrix<double> a = Matrix<double>.Build.SparseOfArray(A);
            Matrix<double> b = Matrix<double>.Build.SparseOfArray(B);
            Matrix<double> c = a * b;


            if (isTransponse)
            {
                return c.Transpose().ToArray();
            }
            else
            {
                return c.ToArray();
            }
        }
    }
}
