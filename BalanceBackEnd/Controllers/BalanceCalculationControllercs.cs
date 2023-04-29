using BalanceBackEnd.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BalanceBackEnd.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BalanceCalculationControllercs : Controller
    {
        [HttpPost("calculate")]
        public ActionResult<double[]> CalculateIntegral([FromBody] BalanceInput data)
        {

            BalanceService balance = new BalanceService();

            var solver = balance.calcBalance(data);

            return solver.Solution;
        }

    }
}
