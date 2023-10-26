namespace BalanceBackEnd.Report
{

    public interface IBalanceReportBuilder
    {
        IBalanceReportBuilder BuildHeader();

        IBalanceReportBuilder BuildBody();

        IBalanceReportBuilder BuildFooter();

        BalanceReport GetReport();
    }
}
