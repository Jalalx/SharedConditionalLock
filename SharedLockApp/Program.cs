using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLockApp
{
    class Program
    {
        private static volatile object _sharedLock = new object();

        static void Main(string[] args)
        {
            var workerTasks = Enumerable.Range(1, 10).Select(x => Task.Run(() => AddPayment())).ToArray();
            Task.WaitAll(workerTasks);

            Console.WriteLine("************************ Insertion completed.");

            var financialItemList = new FinancialService().GetAll();
            foreach (var financial in financialItemList)
            {
                Console.WriteLine($"{financial.Type} #{financial.Number}");
            }

            Console.WriteLine($"Lock length is {SharedConditionalLock.LockLength}");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static Random random = new Random();
        private static void AddPayment()
        {
            for (var i = 0; i < 10; i++)
            {
                var service = new FinancialService();

                var newFinancial = random.Next(0, 9) % 2 == 0 ? (FinancialObject)new Payment() : new Invoice();

                var newNumber = service.GetNewNumber(newFinancial.Type);
                Console.WriteLine($"Getting a new number {newNumber} by thread {Thread.CurrentThread.ManagedThreadId}");

                using (var sharedLock = new SharedConditionalLock(true, newFinancial.Type.ToString()))
                {
                    newFinancial.Number = service.GetNewNumber(newFinancial.Type);
                    Console.WriteLine($"A new {newFinancial.Type} number {newFinancial.Number} set for save by thread {Thread.CurrentThread.ManagedThreadId}.");

                    service.Add(newFinancial);
                    Console.WriteLine($"{newFinancial.Type} with number {newFinancial.Number} added by thread {Thread.CurrentThread.ManagedThreadId}.");
                }
            }
        }
    }
}
