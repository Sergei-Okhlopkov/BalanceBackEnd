namespace BalanceBackEnd.Entities
{
    public class TreeElement
    {
        public TreeElement()
        {
        }

        public TreeElement(List<(int, int, int, string)> flows, double globalTestValue)
        {
            Flows = flows;
            GlobalTestValue = globalTestValue;
        }

        public Guid Id { get; } = Guid.NewGuid();

        public List<(int, int, int, string)> Flows { get; } = new List<(int, int, int, string)>();

        public double GlobalTestValue { get; set; }
       
    }
}
