using BalanceBackEnd;
using BalanceBackEnd.Controllers;
using BalanceBackEnd.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TreeCollections;

namespace BalanceUnitTest
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void BalanceService_is_working_right()
        {
            int numFlows = 600;
            int numNodes = 400;

            BalanceInput BI = Generator.GenerateFlows(numFlows, numNodes);

            BalanceService.Instance.LoadData(BI);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var solver = BalanceService.Instance.calcBalance();
            sw.Stop();

            double time = sw.ElapsedMilliseconds / 1000.0;

            Assert.AreEqual(solver.Status.ToString(), "Success");

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Genrator_flows_les_than_nodes_plus_one()
        {
            int numFlows = 700;
            int numNodes = 800;

            BalanceInput BI = Generator.GenerateFlows(numFlows, numNodes);

            BalanceService.Instance.LoadData(BI);
            Stopwatch sw = new Stopwatch();

            var solver = BalanceService.Instance.calcBalance();

        }

        [TestMethod]
        public void Balancing_ORIGINAL()
        {
                       
            BalanceInput BI = new BalanceInput();

            #region Задание потоков по модели

            List<Flow> Flows = new List<Flow>();

            //1
            Flows.Add(new Flow {
                Id = Guid.NewGuid(),
                Out = 0,
                In = 1,
                Tolerance = 0.200,
                Measured = 10.0054919341489,
                Min = 0,
                Max = 1000
            });

            //2
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 1,
                In = 0,
                Tolerance = 0.121,
                Measured = 3.03265795024749,
                Min = 0,
                Max = 1000
            });

            //3
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 1,
                In = 2,
                Tolerance = 0.683,
                Measured = 6.83122010827837,
                Min = 0,
                Max = 1000
            });

            //4
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 2,
                In = 0,
                Tolerance = 0.040,
                Measured = 1.98478460320379,
                Min = 0,
                Max = 1000
            });

            //5
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 2,
                In = 3,
                Tolerance = 0.102,
                Measured = 5.09293357450987,
                Min = 0,
                Max = 1000
            });

            //6
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 3,
                In = 0,
                Tolerance = 0.081,
                Measured = 4.05721328676762,
                Min = 0,
                Max = 1000
            });

            //7
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 3,
                In = 0,
                Tolerance = 0.020,
                Measured = 0.991215230484718,
                Min = 0,
                Max = 1000
            });

            BI.Flows = Flows;

            #endregion

            BalanceService.Instance.LoadData(BI);
            var solver = BalanceService.Instance.calcBalance();

            double[] expected = new double[]
            { 10.0555820555568, 3.0142509913592, 7.04133106419761, 1.98210405567851, 5.0592270085191, 4.06740355102557, 0.991823457493527 };

            double delta = 0.01;

            for (int i = 0; i < solver.Solution.Length; i++)
            {
                Assert.AreEqual(expected[i], solver.Solution[i], delta);
            }

        }

        [TestMethod]
        public void Balancing_Errors_Test_V1()
        {
            
            BalanceInput BI = new BalanceInput();

            #region Задание потоков по модели

            List<Flow> Flows = new List<Flow>();

            //1
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 0,
                In = 1,
                Tolerance = 0.200,
                Measured = 10.0054919341489,
                Min = 0,
                Max = 1000
            });

            //2
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 1,
                In = 0,
                Tolerance = 0.121,
                Measured = 3.03265795024749,
                Min = 0,
                Max = 1000
            });

            //3
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 1,
                In = 2,
                Tolerance = 0.683,
                Measured = 6.83122010827837,
                Min = 0,
                Max = 1000
            });

            //4
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 2,
                In = 0,
                Tolerance = 0.040,
                Measured = 1.98478460320379,
                Min = 0,
                Max = 1000
            });

            //5
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 2,
                In = 3,
                Tolerance = 0.102,
                Measured = 5.09293357450987,
                Min = 0,
                Max = 1000
            });

            //6
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 3,
                In = 0,
                Tolerance = 0.081,
                Measured = 4.05721328676762,
                Min = 0,
                Max = 1000
            });

            //7
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 3,
                In = 0,
                Tolerance = 0.020,
                Measured = 0.991215230484718,
                Min = 0,
                Max = 1000
            });

            //8
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 1,
                In = 0,
                Tolerance = 0.667,
                Measured = 6.66666,
                Min = 0,
                Max = 1000
            });

            BI.Flows = Flows;

            #endregion

            BalanceService.Instance.LoadData(BI);
            var solver = BalanceService.Instance.calcBalance();

            double[] expected = new double[]
            { 10.5402456913603, 2.83614833622227, 6.97261297632469, 1.9632643552402, 5.0093486210845, 4.02033457294678, 0.989014048137712, 0.731484378813389 };

            double delta = 0.01;

            for (int i = 0; i < solver.Solution.Length; i++)
            {
                Assert.AreEqual(expected[i], solver.Solution[i], delta);
            }

        }

        [TestMethod]
        public void ControllerBalance_Test_Output()
        {
            BalanceInput BI = new BalanceInput();

            #region Задание потоков по модели

            List<Flow> Flows = new List<Flow>();

            //1
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 0,
                In = 1,
                Tolerance = 0.200,
                Measured = 10.0054919341489,
                Min = 0,
                Max = 1000
            });

            //2
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 1,
                In = 0,
                Tolerance = 0.121,
                Measured = 3.03265795024749,
                Min = 0,
                Max = 1000
            });

            //3
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 1,
                In = 2,
                Tolerance = 0.683,
                Measured = 6.83122010827837,
                Min = 0,
                Max = 1000
            });

            //4
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 2,
                In = 0,
                Tolerance = 0.040,
                Measured = 1.98478460320379,
                Min = 0,
                Max = 1000
            });

            //5
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 2,
                In = 3,
                Tolerance = 0.102,
                Measured = 5.09293357450987,
                Min = 0,
                Max = 1000
            });

            //6
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 3,
                In = 0,
                Tolerance = 0.081,
                Measured = 4.05721328676762,
                Min = 0,
                Max = 1000
            });

            //7
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 3,
                In = 0,
                Tolerance = 0.020,
                Measured = 0.991215230484718,
                Min = 0,
                Max = 1000
            });

            //8
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 1,
                In = 0,
                Tolerance = 0.667,
                Measured = 6.66666,
                Min = 0,
                Max = 1000
            });

            BI.Flows = Flows;

            #endregion
            var controller = new BalanceCalculationControllers();
            /*string data;

            using (StreamReader sr = new StreamReader("test data.txt"))
            {
                data = sr.ReadToEnd();
            }*/

            var result = controller.CalculateIntegral(BI);

            Assert.IsInstanceOfType(result, typeof(ActionResult<double[]>));
        }

        [TestMethod]
        public void GTValue_Test()
        {
            BalanceInput BI = new BalanceInput();

            #region Задание потоков по модели

            List<Flow> Flows = new List<Flow>();

            //1
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 0,
                In = 1,
                Tolerance = 0.200,
                Measured = 10.0054919341489,
                Min = 0,
                Max = 1000
            });

            //2
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 1,
                In = 0,
                Tolerance = 0.121,
                Measured = 3.03265795024749,
                Min = 0,
                Max = 1000
            });

            //3
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 1,
                In = 2,
                Tolerance = 0.683,
                Measured = 6.83122010827837,
                Min = 0,
                Max = 1000
            });

            //4
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 2,
                In = 0,
                Tolerance = 0.040,
                Measured = 1.98478460320379,
                Min = 0,
                Max = 1000
            });

            //5
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 2,
                In = 3,
                Tolerance = 0.102,
                Measured = 5.09293357450987,
                Min = 0,
                Max = 1000
            });

            //6
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 3,
                In = 0,
                Tolerance = 0.081,
                Measured = 4.05721328676762,
                Min = 0,
                Max = 1000
            });

            //7
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 3,
                In = 0,
                Tolerance = 0.020,
                Measured = 0.991215230484718,
                Min = 0,
                Max = 1000
            });

            BI.Flows = Flows;

            #endregion

            BalanceService.Instance.LoadData(BI);

            double GT = BalanceService.Instance.GlobalTest(BI);

            bool condition = GT < 1;

            Assert.AreEqual(true, condition);
        }


        [TestMethod]

        public void GLR_Test()
        {
            BalanceInput BI = new BalanceInput();

            #region Задание потоков по модели

            List<Flow> Flows = new List<Flow>();

            //1
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 0,
                In = 1,
                Name = "X1",
                Tolerance = 0.200,
                Measured = 100.0054919341489,
                Min = 0,
                Max = 1000
            });

            //2
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 1,
                In = 0,
                Name = "X2",
                Tolerance = 0.121,
                Measured = 3.03265795024749,
                Min = 0,
                Max = 1000
            });

            //3
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 1,
                In = 2,
                Name = "X3",
                Tolerance = 0.683,
                Measured = 6.83122010827837,
                Min = 0,
                Max = 1000
            });

            //4
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 2,
                In = 0,
                Name = "X4",
                Tolerance = 0.040,
                Measured = 1.98478460320379,
                Min = 0,
                Max = 1000
            });

            //5
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 2,
                In = 3,
                Name = "X5",
                Tolerance = 0.102,
                Measured = 5.09293357450987,
                Min = 0,
                Max = 1000
            });

            //6
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 3,
                In = 0,
                Name = "X6",
                Tolerance = 0.081,
                Measured = 4.05721328676762,
                Min = 0,
                Max = 1000
            });

            //7
            Flows.Add(new Flow
            {
                Id = Guid.NewGuid(),
                Out = 3,
                In = 0,
                Name = "X7",
                Tolerance = 0.020,
                Measured = 0.991215230484718,
                Min = 0,
                Max = 1000
            });

            BI.Flows = Flows;

            #endregion
            MutableEntityTreeNode<Guid, TreeElement> treeNode;
            List<(int Input, int Output, int FlowNum, string FlowName)> list;

            BalanceService.Instance.LoadData(BI);

            BalanceService.Instance.LoadData(BI);

            (treeNode, list) = BalanceService.Instance.StartGlr();
            
            double GT = 0.05;

            bool condition = GT < 1;

            Assert.AreEqual(true, condition);
        }
    }
}