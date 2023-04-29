using BalanceBackEnd.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BalanceUnitTest
{
    public static class Generator
    {
        public static BalanceInput GenerateFlows(int numFlows, int numNodes)
        {
            if (numFlows < numNodes + 1)
                throw new ArgumentException("Число потоков не может быть меньше чем число узлов + 1 для генерируемой системы");

            List<Flow> Flows = new List<Flow>();

            //заполнение 
            //первый этап - сделаем модель "поровозик"

           
            Random random = new Random(unchecked((int)(DateTime.Now.Ticks)));
           

            for (int i = 0; i < numNodes; i++)
            {
                
                Flows.Add(new Flow
                {
                    Id = Guid.NewGuid(),
                    Out = i,
                    In = i + 1,
                    Tolerance = Math.Round(random.NextDouble(), 6),
                    Measured = Math.Round(random.NextDouble() * 10, 6),
                    Min = 0,
                    Max = 1000
                });
            }

           
            Flows.Add(new Flow
            {
                //Добавляем поток, который выходит в среду (значение 0 )
                Id = Guid.NewGuid(),
                Out = numNodes,
                In = 0,
                Tolerance = Math.Round(random.NextDouble(), 6),
                Measured = Math.Round(random.NextDouble() * 10, 6),
                Min = 0,
                Max = 1000
            });

            //2-ой этап - раскидываем отсавшиеся потоки
            for (int i = 0; i < numFlows - numNodes - 1; i++)
            {


                Flows.Add(new Flow
                {
                    Id = Guid.NewGuid(),
                    Out = random.Next() % (numNodes + 1),
                    In = random.Next() % (numNodes + 1),
                    Tolerance = Math.Round(random.NextDouble(), 6),
                    Measured = Math.Round(random.NextDouble() * 10, 6),
                    Min = 0,
                    Max = 1000
                });
            }

            BalanceInput BI = new BalanceInput();
            BI.Flows = Flows;



            return BI;
        }
    }
}
