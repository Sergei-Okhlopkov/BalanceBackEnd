using Accord.Math;
using Accord.Math.Optimization;
using BalanceBackEnd.Entities;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


namespace BalanceBackEnd
{
    public class BalanceService
    {

        public GoldfarbIdnani? calcBalance(BalanceInput balanceInput)
        {
            int threadNum = balanceInput.Flows.Count;

            int numberOfEqualities = GetNumThreads(balanceInput); //количество узлов

            double[,] W = new double[threadNum, threadNum];
            for (int i = 0; i < threadNum; i++)
            {
                for (int j = 0; j < threadNum; j++)
                {
                    if (i == j)
                        W[i, j] = 1 / (balanceInput.Flows[i].Tolerance * balanceInput.Flows[i].Tolerance);
                    else W[i, j] = 0;
                }
            }

            double[,] I = new double[threadNum, threadNum];
            for (int i = 0; i < threadNum; i++)
            {
                for (int j = 0; j < threadNum; j++)
                {
                    if (i == j)
                        I[i, j] = 1;
                    else I[i, j] = 0;
                }
            }

            double[] x0 = new double[threadNum];
            for (int i = 0; i < threadNum; i++)
            {
                x0[i] = balanceInput.Flows[i].Measured;
            }

            double[] b = new double[numberOfEqualities + threadNum * 2]; // 3 - число условий g кол-ву узлов
                                                                         // threadNum - число условий по кол-ву точек  * 2
            for (int j = 0; j < numberOfEqualities; j++)
            {
                b[j] = 0;
            }

            //боксовые ограничения
            for (int j = 0; j < threadNum * 2; j += 2)
            {
                b[j + numberOfEqualities] = balanceInput.Flows[j / 2].Min;
                b[j + numberOfEqualities + 1] = -balanceInput.Flows[j / 2].Max;
                /*if (balanceInput.arrowInput[j / 2].Min >= balanceInput.arrowInput[j / 2].Max)
                    result.exeption.Add("Некорректные данные в потоке " + (j / 2 + 1)); //Надо сделать ошибки через ошибку 500*/
            }

            double[,] A = new double[numberOfEqualities + threadNum * 2, threadNum];
            double[,] AGT = new double[numberOfEqualities, threadNum];
            for (int i = 0; i < numberOfEqualities; i++)
            {
                for (int j = 0; j < threadNum; j++)
                {
                    if (i + 1 == balanceInput.Flows[j].Out)
                    {
                        A[i, j] = -1;
                        AGT[i, j] = -1;
                    }
                    else if (i + 1 == balanceInput.Flows[j].In)
                    {
                        A[i, j] = 1;
                        AGT[i, j] = 1;
                    }
                    else
                    {
                        A[i, j] = 0;
                        AGT[i, j] = 0;
                    }
                }
            }
            int k = 3;
            for (int i = 0; i < threadNum; i++)
            {
                A[k, i] = 1;
                A[k + 1, i] = -1;
                k += 2;
            }

            var H = MatrixMultiplicator.MultiplyDenseMatrixes(I, W);

            Matrix<double> Mh = -Matrix<double>.Build.DenseOfArray(H);
            double[] d = Mh.Multiply(DenseVector.Build.DenseOfArray(x0)).ToArray();

            var temp = new SparseVector(threadNum);
            var temp2 = new SparseVector(threadNum);
            for (int i = 0; i < threadNum; i++)
            {
                temp[i] = I[i, i];
                temp2[i] = Math.Sqrt((1 / W[i, i]));

            }
            var measurability = temp.ToArray();
            var tolerance = temp2.ToArray();
            double GT = StartGlobalTest(x0, AGT, measurability, tolerance);

            var solver = new GoldfarbIdnani(H, d, A, b, numberOfEqualities);
            solver.Minimize();

            return solver;
        }

        public double StartGlobalTest(double[] x0, double[,] a, double[] measurability, double[] tolerance)
        {
            var aMatrix = SparseMatrix.OfArray(a); 
            var aTransposedMatrix = SparseMatrix.OfMatrix(aMatrix.Transpose());
            var x0Vector = SparseVector.OfEnumerable(x0);

            // Введение погрешностей по неизмеряемым потокам
            var xStd = SparseVector.OfEnumerable(tolerance) / 1.96;

            for (var i = 0; i < xStd.Count; i++)
            {
                /*(Math.Abs(measurability[i]) < 0.0000001)*/
                if (measurability[i] == 0.0)
                {
                    xStd[i] = Math.Pow(10, 2) * x0Vector.Maximum();
                }
            }

            var sigma = SparseMatrix.OfDiagonalVector(xStd.PointwisePower(2));
            // Вычисление вектора дисбалансов
            var r = aMatrix * x0Vector;
            var v = aMatrix * sigma * aTransposedMatrix;
            var vv = v.ToArray();
            vv = vv.PseudoInverse();
            v = SparseMatrix.OfArray(vv);
            var result = r * v * r.ToColumnMatrix();
            var chi = ChiSquared.InvCDF(aMatrix.RowCount, 1 - 0.05);
            // нормирование
            return result[0] / chi;
        }

        /// <summary>
        /// Функция возвращает количество узлов в системе, анализируя потоки
        /// </summary>
        /// <param name="balanceInput"></param>
        /// <returns></returns>
        private int GetNumThreads(BalanceInput balanceInput)
        {

            Dictionary<int, bool> knot = new Dictionary<int, bool>();

            foreach (var thread in balanceInput.Flows)
            {
                if (thread.In != 0) knot.TryAdd(thread.In, true);

                if (thread.Out != 0) knot.TryAdd(thread.Out, true);
            }


            return knot.Count;
        }
    }
}
