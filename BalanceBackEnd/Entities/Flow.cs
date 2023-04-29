namespace BalanceBackEnd.Entities
{

    public class Flow
    {
        public Guid Id { get; set; } //оригинальный ID потока
        public int In { get; set; } //номер узла, в который входит данный поток
        public int Out { get; set; } //номер узла, из которого выходит данный поток
        public double Tolerance { get; set; } //погрешность измерения потока
        public double Measured { get; set; } //измеренное значение потока (x0)
        public double Min { get; set; }
        public double Max { get; set; }
    }
}
