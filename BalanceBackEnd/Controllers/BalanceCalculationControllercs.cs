using BalanceBackEnd.Entities;
using BalanceBackEnd.Report;
using BalanceUnitTest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Diagnostics;
using TreeCollections;

namespace BalanceBackEnd.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BalanceCalculationControllers : Controller
    {
        private static Logger logger = LogManager.GetLogger("BalanceMainLogger");

        [HttpPost("calculate")]
        public ActionResult<double[]> CalculateBalance([FromBody] BalanceInput data)
        {

            BalanceService.Instance.LoadData(data);

            var solver = BalanceService.Instance.calcBalance();
            logger.Info("баланс расчитан");

            return solver.Solution;
        }

        [HttpGet("calculateReport")]
        public ActionResult<string> ReportFromSomeBalances(int numBalances)
        {
            List<Balance> balances = new List<Balance>();

            for (int i = 0; i < numBalances; i++)
            {
                Random rand = new Random();
                int numNodes = rand.Next() % 10 + 5;
                int numFlows = rand.Next() % 20 + numNodes + 1;

                BalanceInput data = Generator.GenerateFlows(numFlows, numNodes);
                BalanceService.Instance.LoadData(data);
                var solver = BalanceService.Instance.calcBalance();
                logger.Info("баланс расчитан");
                double[] solution = solver.Solution;

                balances.Add(new Balance()
                {
                    Solution = solution,
                    FlowsCount = numFlows,
                    NodeCount = numNodes
                });

            }

            var builder = new BalanceReportBuilder(balances);

            builder
                .BuildHeader()
                .BuildBody()
                .BuildFooter();

            var report = builder.GetReport();
            
            return report.ToString();
        }

        [HttpGet("profiler")]
        public ActionResult<double[]> ProfilerTest()
        {
            int numFlows = 600;
            int numNodes = 300;

            BalanceInput BI = Generator.GenerateFlows(numFlows, numNodes);

            BalanceService.Instance.LoadData(BI);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var solver = BalanceService.Instance.calcBalance();
            sw.Stop();

            double time = sw.ElapsedMilliseconds / 1000.0;

            logger.Info("баланс расчитан на Profiler");

            return solver.Solution;
        }

        [HttpPost("getGT")]
        public ActionResult<double> GlobalTest([FromBody] BalanceInput data)
        {
            return new BalanceService().GlobalTest(data);
        }

        [HttpPost("getGLR")]
        public ActionResult<(MutableEntityTreeNode<Guid, TreeElement>, List<(int Input, int Output, int FlowNum, string FlowName)>)> GLR([FromBody] BalanceInput data)
        {
            BalanceService.Instance.LoadData(data);
            return BalanceService.Instance.StartGlr();
        }

    }
}
