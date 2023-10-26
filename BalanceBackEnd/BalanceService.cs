using Accord.Math;
using Accord.Math.Optimization;
using BalanceBackEnd.Entities;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NLog;
using System.Collections.Concurrent;
using TreeCollections;

namespace BalanceBackEnd
{
    public class BalanceService
    {

        private int threadNum;
        private int numberOfEqualities;
        private BalanceInput balanceInput;
        private ConcurrentStack<BalanceInput> foundDecisions;
        private int searchDeapth;
        private int searchWidth;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static BalanceService _instance;

        public static BalanceService Instance
        {
            get
            {
                return _instance ?? (_instance = new BalanceService());
            }
        }


        public void LoadData(BalanceInput balanceInput)
        {
            this.balanceInput = balanceInput;
            this.threadNum = balanceInput.Flows.Count;
            this.numberOfEqualities = GetNumThreads(balanceInput); //количество узлов
        }

        public GoldfarbIdnani? calcBalance()
        {
            logger.Info("Начало расчёта баланса");
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

            logger.Info("Задана матрица I");

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

            for (int i = 0; i < numberOfEqualities; i++)
            {
                for (int j = 0; j < threadNum; j++)
                {
                    if (i + 1 == balanceInput.Flows[j].Out)
                    {
                        A[i, j] = -1;
                    }
                    else if (i + 1 == balanceInput.Flows[j].In)
                    {
                        A[i, j] = 1;
                    }
                    else
                    {
                        A[i, j] = 0;
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

            var solver = new GoldfarbIdnani(H, d, A, b, numberOfEqualities);
            solver.Minimize();

            return solver;
        }

        public double GlobalTest(BalanceInput balanceInput)
        {


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

            double[,] AGT = new double[numberOfEqualities, threadNum]; //матрица для прохождения глобального теста. A не подходит, так как содержит
                                                                       //дополнительные строки с ограничениями, которые порят расчёт
            for (int i = 0; i < numberOfEqualities; i++)
            {
                for (int j = 0; j < threadNum; j++)
                {
                    if (i + 1 == balanceInput.Flows[j].Out)
                    {
                        AGT[i, j] = -1;
                    }
                    else if (i + 1 == balanceInput.Flows[j].In)
                    {
                        AGT[i, j] = 1;
                    }
                    else
                    {
                        AGT[i, j] = 0;
                    }
                }
            }

            var temp = new SparseVector(threadNum);
            var temp2 = new SparseVector(threadNum);
            for (int i = 0; i < threadNum; i++)
            {
                temp[i] = I[i, i];
                temp2[i] = Math.Sqrt((1 / W[i, i]));

            }
            var measurability = temp.ToArray();
            var tolerance = temp2.ToArray();

            return StartGlobalTest(x0, AGT, measurability, tolerance);
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
            vv = vv.PseudoInverse(); //Псевдоинверсия
            v = SparseMatrix.OfArray(vv);
            var result = r * v * r.ToColumnMatrix(); // GT_ORIGINAL
            var chi = ChiSquared.InvCDF(aMatrix.RowCount, 1 - 0.05); // хи-квадрат (Степени свободы, 1-alpha), он же GT_LIMIT
            // нормирование
            return result[0] / chi;
        }

        public (double[,], List<(int Input, int Output, int FlowNum, string FlowName)>) GlrTest(double[] x0, double[,] a, double[] measurability, double[] tolerance,
           List<(int, int, int, string)> flows, int countOfThreads, double globalTest)
        {

            DenseVector max = new DenseVector(threadNum);
            DenseVector min = new DenseVector(threadNum);

            for (int i = 0; i < threadNum; i++)
            {
                max[i] = balanceInput.Flows[i].Max;
                min[i] = balanceInput.Flows[i].Min;
            }
            var nodesCount = a.GetLength(0);
            double[] corr = new double[countOfThreads];
            var glrTable = new double[nodesCount, nodesCount];

            foreach (var flow in flows)
            {
                var sum = 0.0;
                var correction = 0.0;
                var (i, j, l, _) = flow;

                // Добавляем новый поток в схеме
                var aColumn = new double[nodesCount];
                aColumn[i] = -1;
                aColumn[j] = 1;

                var aNew = a.InsertColumn(aColumn);

                var aRow = new double[x0.Length];
                for (int k = 0; k < x0.Length; k++)
                    aRow[k] = a[i, k];
                for (int k = 0; k < x0.Length; k++)
                {
                    if (k == l)
                        continue;
                    else
                    {
                        if (aRow[k] == 1)
                            sum += x0[k];
                        else if (aRow[k] == -1)
                            sum -= x0[k];
                    }
                }
                if (sum > 0.0) correction -= sum;
                else correction += sum;
                corr[l] = correction;

                var asdf = x0[l] + corr[l];

                if ((x0[l] + corr[l]) < min[l] || (x0[l] + corr[l]) > max[l])
                {
                    continue;
                }

                var x0New = x0.Append(0).ToArray();

                var measurabilityNew = measurability.Append(0).ToArray();
                var toleranceNew = tolerance.Append(0).ToArray();

                // Считаем тест и находим разницу
                glrTable[i, j] = globalTest - StartGlobalTest(x0New, aNew, measurabilityNew, toleranceNew);
            }


            return (glrTable, flows);
        }

        public (MutableEntityTreeNode<Guid, TreeElement>, List<(int Input, int Output, int FlowNum, string FlowName)>) StartGlr()
        {

            DateTime CalculationTimeStart;
            DateTime CalculationTimeFinish;

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
            double[,] AGT = new double[numberOfEqualities, threadNum]; //матрица для прохождения глобального теста. A не подходит, так как содержит
                                                                       //дополнительные строки с ограничениями, которые порят расчёт
            for (int i = 0; i < numberOfEqualities; i++)
            {
                for (int j = 0; j < threadNum; j++)
                {
                    if (i + 1 == balanceInput.Flows[j].Out)
                    {
                        AGT[i, j] = -1;
                    }
                    else if (i + 1 == balanceInput.Flows[j].In)
                    {
                        AGT[i, j] = 1;
                    }
                    else
                    {
                        AGT[i, j] = 0;
                    }
                }
            }


            DenseVector max = new DenseVector(threadNum);
            DenseVector min = new DenseVector(threadNum);

            for (int i = 0; i < threadNum; i++)
            {
                max[i] = balanceInput.Flows[i].Max;
                min[i] = balanceInput.Flows[i].Min;
            }


            //var techU = technologicRangeUpperBound.ToArray();
            //var techL = technologicRangeLowerBound.ToArray();

            var temp = new SparseVector(threadNum);
            var temp2 = new SparseVector(threadNum);
            for (int i = 0; i < threadNum; i++)
            {
                temp[i] = I[i, i];
                temp2[i] = Math.Sqrt((1 / W[i, i]));

            }
            var measurability = temp.ToArray();
            var tolerance = temp2.ToArray();

            var flows = GetExistingFlows(AGT).ToList();
            var nodesCount = AGT.Rows();
            var rootNode = new MutableEntityTreeNode<Guid, TreeElement>(x => x.Id, new TreeElement());
            var analyzingNode = rootNode;
            while (analyzingNode != null)
            {
                var newMeasurability = measurability;
                var newTolerance = tolerance;
                var newA = AGT;
                var newX0 = x0;
                var newMax = max;
                var newMin = min;
                //var newtechU = techU;
                //var newtechL = techL;

                var H = MatrixMultiplicator.MultiplyDenseMatrixes(I, W);
                var newh = H.ToArray();
                Matrix<double> Mh = -Matrix<double>.Build.DenseOfArray(H);
                double[] d = Mh.Multiply(DenseVector.Build.DenseOfArray(x0)).ToArray();
                var newD = d;
                //Добавляем уже сущ. потоки от родителя
                foreach (var (newI, newJ, newNum, newName) in analyzingNode.Item.Flows)
                {
                    var aColumn = new double[nodesCount];
                    aColumn[newI] = 1;
                    aColumn[newJ] = -1;

                    newMeasurability = newMeasurability.Append(0).ToArray();
                    newTolerance = newTolerance.Append(0).ToArray();

                    newX0 = newX0.Append(0).ToArray();
                    newMax = newMax.Append(max[newNum]).ToArray();
                    newMin = newMin.Append(min[newNum]).ToArray();
                    //newtechU = newtechU.Append(technologicRangeUpperBound[newNum]).ToArray();
                    //newtechL = newmetrL.Append(technologicRangeLowerBound[newNum]).ToArray();
                    var hColumn = new double[nodesCount + 1];
                    var hRow = new double[newX0.Length];
                    foreach (int elem in hColumn)
                        hColumn[elem] = 0;
                    foreach (int elem in hRow)
                        hRow[elem] = 0;
                    newh = newh.InsertColumn(hColumn);
                    newh = newh.InsertRow(hRow);
                    newD = newD.Append(0).ToArray();
                    newA = newA.InsertColumn(aColumn);
                }
                CalculationTimeStart = DateTime.Now;
                //Значение глобального теста
                var gTest = StartGlobalTest(newX0, newA, newMeasurability, newTolerance);

                //GLR
                var (glr, fl) = GlrTest(newX0, newA, newMeasurability, newTolerance, flows, threadNum, gTest);
                //var (glr, fl) = ParallelGlrTest(newX0, newA, newMeasurability, newTolerance, flows, gTest);
                var (i, j) = glr.ArgMax();
                var ijvalue = glr[i, j];
                //TODO: расчёт баланса на Accord.NET вставить

                //var check = BalanceGurobiForGLR(newX0, newA, newh, newD, newtechL, newtechU, newmetrL, newmetrU);
                if (gTest >= 0.05)
                //if (gTest >= 0.005)
                {
                    var flowIndex = fl[fl.FindIndex(x => x.Input == i && x.Output == j)].FlowNum;
                    var flowName = fl[fl.FindIndex(x => x.Input == i && x.Output == j)].FlowName;
                    var fname = balanceInput.Flows[flowIndex].Name;

                    var node = new TreeElement(new List<(int, int, int, string)>(analyzingNode.Item.Flows), gTest);
                    analyzingNode = analyzingNode.AddChild(node);
                    node.Flows.Add((i, j, flowIndex, flowName));



                }
                else
                {
                    CalculationTimeFinish = DateTime.Now;
                    //GlrTime = (CalculationTimeFinish - CalculationTimeStart).TotalSeconds;
                    analyzingNode.Item.GlobalTestValue = gTest;
                    analyzingNode = null;
                }
            }
            return (rootNode, flows);
        }

        public ICollection<(int Input, int Output, int FlowNum, string FlowName)> GetExistingFlows(double[,] a)
        {
            var flows = new List<(int, int, int, string)>();
            for (var k = 0; k < a.Columns(); k++)
            {
                var column = a.GetColumn(k);

                var i = column.IndexOf(-1);
                var j = column.IndexOf(1);

                if (i == -1 || j == -1)
                {
                    continue;
                }
                var fname = balanceInput.Flows[k].Name;
                flows.Add((i, j, k, fname));
            }

            return flows;
        }

        //public void FixModel(BalanceInput data, List<BasicSchemeGT> result, int maxDepth = 1000, int maxWidth = 1000, int currentDepth = 1)
        //{
        //    var stack = new ConcurrentStack<BalanceInput>(result);
        //    FixModel(data, stack, maxDepth, maxWidth, currentDepth);
        //    result.AddRange(stack);
        //}

        //public void FixModel(BasicScheme data, ConcurrentStack<BasicSchemeGT> result, int maxDepth = 1000, int maxWidth = 1000, int currentDepth = 1)
        //{
        //    double originalScore = ConductGlobalTest(data.AdjacencyMatrix, data.Flows, data.AbsoluteTolerance, data.Measurability);

        //    if (originalScore <= 1)
        //    {
        //        result.Push(new BasicSchemeGT(data, originalScore));
        //        return;
        //    }

        //    if (currentDepth <= maxDepth)
        //    {
        //        var nodes = GetNodeAdjMatrix(data.AdjacencyMatrix);
        //        var glrMatrix = GetGLRScores(data.AdjacencyMatrix, data.Flows, data.AbsoluteTolerance, data.Measurability, nodes, originalScore);
        //        int a = data.AdjacencyMatrix.GetLength(0);
        //        int b = data.AdjacencyMatrix.GetLength(1);

        //        var inds = nodes.GetIndices().ToArray();

        //        inds = inds.OrderByDescending(i => glrMatrix[i[0], i[1]]).ToArray();
        //        int currentMaxWidth = maxWidth;

        //        var res = Parallel.For(0, inds.Length, (int i) =>
        //        {
        //            if (i >= currentMaxWidth || i % 2 == 1)
        //            {
        //                return;
        //            }

        //            if (inds[i][0] == inds[i][1])
        //            {
        //                currentMaxWidth += 2;
        //                return;
        //            }

        //            if (glrMatrix[inds[i][0], inds[i][1]] > 0)
        //            {
        //                var newData = new BasicScheme(a, b + 1);
        //                newData.CopyValues(data.AdjacencyMatrix, data.Flows, data.AbsoluteTolerance, data.Measurability);
        //                newData.AdjacencyMatrix[inds[i][0], b] = 1;
        //                newData.AdjacencyMatrix[inds[i][1], b] = -1;

        //                FixModel(newData, result, maxDepth, maxWidth, currentDepth + 1);
        //            }
        //        });

        //        while (!res.IsCompleted)
        //        {
        //            Thread.Sleep(1);
        //        }
        //    }

        //    return;
        //}

        protected double[,] GetGLRScores(BalanceInput newData, bool[,] nodes, double originalScore)
        {
            int size = newData.Flows.Count;
            //int a = adjacencyMatrix.GetLength(0);
            //int b = adjacencyMatrix.GetLength(1);
            var glrM = new double[size, size];

            int[,] adjNew = new int[size, size + 1];
            //double[] flowsNew = new double[b + 1];
            //double[] absTolNew = new double[b + 1];
            //int[] measNew = new int[b + 1];

            //adjacencyMatrix.CopyTo(adjNew);
            //flows.CopyTo(flowsNew);
            //absoluteTolerance.CopyTo(absTolNew);
            //measurability.CopyTo(measNew);

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (nodes[i, j])
                    {
                        adjNew[i, size] = 1;
                        adjNew[j, size] = -1;

                        glrM[i, j] = originalScore - GlobalTest(newData);
                        glrM[j, i] = glrM[i, j];

                        adjNew[i, size] = 0;
                        adjNew[j, size] = 0;
                    }
                }
            }

            return glrM;
        }

        protected bool[,] GetNodeAdjMatrix(int[,] adjacencyMatrix)
        {
            int a = adjacencyMatrix.GetLength(0);
            int b = adjacencyMatrix.GetLength(1);
            bool[,] res = new bool[a, a];


            for (int i = 0; i < a; i++)
            {
                res[i, i] = true;
                for (int j = 0; j < i; j++)
                {
                    for (int k = 0; k < b; k++)
                    {
                        if (adjacencyMatrix[i, k] == 1 && adjacencyMatrix[j, k] == -1 || adjacencyMatrix[i, k] == -1 && adjacencyMatrix[j, k] == 1)
                        {
                            res[i, j] = true;
                            res[j, i] = true;
                            break;
                        }
                    }
                }
            }

            return res;
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
