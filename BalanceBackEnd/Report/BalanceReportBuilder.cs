namespace BalanceBackEnd.Report
{
    public class BalanceReportBuilder : IBalanceReportBuilder
    {
        private BalanceReport _balanceReport;

        private readonly IEnumerable<Balance> _balances;

        public BalanceReportBuilder(IEnumerable<Balance> balances)
        {
            _balances = balances;
            _balanceReport = new();
        }

        public IBalanceReportBuilder BuildHeader()
        {
            _balanceReport.Header =
                $"BALANCE REPORT ON DATE: {DateTime.Now}\n";

            _balanceReport.Header +=
                "\n----------------------------------------------------------------------------------------------------\n";

            return this;
        }

        public IBalanceReportBuilder BuildBody()
        {
            foreach(var balance in _balances)
            {
                _balanceReport.Body += $"\nBalance: Flows: {balance.FlowsCount}\t\tNodes:{balance.NodeCount}\t\t\t\tSolution: ";
                foreach (var item in balance.Solution)
                {
                    _balanceReport.Body += $"{item.ToString("#.##")} ";
                }
            }

            _balanceReport.Body += "\n";

            return this;
        }

        public IBalanceReportBuilder BuildFooter()
        {
            _balanceReport.Footer =
                "\n----------------------------------------------------------------------------------------------------\n";

            _balanceReport.Footer +=
                $"\nTOTAL BALANCES: {_balances.Count()}";

            return this;
        }

        public BalanceReport GetReport()
        {
            BalanceReport balanceReport = _balanceReport;

            _balanceReport = new();

            return balanceReport;
        }

        
    }
}
