using System.Text;

namespace BalanceBackEnd.Report
{
    public class BalanceReport
    {
       
        public string Header { get; set; }

        public string Body { get; set; }

        public string Footer { get; set; }

        public override string ToString() =>
            new StringBuilder()
            .Append(Header)
            .Append(Body)
            .Append(Footer)
            .ToString();
    }
}
