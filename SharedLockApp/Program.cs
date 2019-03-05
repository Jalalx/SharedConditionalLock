using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    public class FinancialService
    {
        private static object sharedLock = new object();
        private static readonly List<FinancialObject> db = new List<FinancialObject>();
        private readonly Random random = new Random();

        public int GetNewNumber(FinancialType type)
        {
            lock (sharedLock)
            {
                //Thread.Sleep(random.Next(10, 1000));
                if (db.Any(x => x.Type == type))
                {
                    return db.Where(x => x.Type == type).OrderByDescending(x => x.Number).First().Number + 1;
                }
                else
                {
                    return 1;
                }
            }
        }

        public void Add(FinancialObject obj)
        {
            lock (sharedLock)
            {
                //Thread.Sleep(random.Next(10, 1000));
                db.Add(obj);
            }
        }

        public IEnumerable<FinancialObject> GetAll()
        {
            //Thread.Sleep(random.Next(10, 1000));
            return db;
        }
    }

    public enum FinancialType
    {
        Invoice = 1,
        Payment = 2
    }

    public abstract class FinancialObject
    {
        public FinancialType Type { get; set; }

        public int Number { get; set; }
    }

    public class Invoice : FinancialObject
    {
        public Invoice()
        {
            Type = FinancialType.Invoice;
        }
    }

    public class Payment : FinancialObject
    {
        public Payment()
        {
            Type = FinancialType.Payment;
        }
    }

    public class SharedConditionalLock : IDisposable
    {
        private class LockInfo
        {
            public int Count = 0;
        }

        private static readonly ConcurrentDictionary<string, LockInfo> _locks = new ConcurrentDictionary<string, LockInfo>();

        public static int LockLength
        {
            get
            {
                return _locks.Count;
            }
        }

        public SharedConditionalLock(bool isEnabled, string key)
        {
            IsEnabled = isEnabled;
            Key = key;

            if (isEnabled)
            {
                var lockInfo = _locks.GetOrAdd(key, new LockInfo());
                Interlocked.Increment(ref lockInfo.Count);
                Monitor.Enter(lockInfo);
            }
        }

        public bool IsEnabled { get; }

        public string Key { get; }

        public void Dispose()
        {
            if (IsEnabled)
            {
                var lockInfo = _locks[Key];
                Monitor.Exit(lockInfo);
                Interlocked.Decrement(ref lockInfo.Count);
                
                if (lockInfo.Count == 0)
                {
                    _locks.TryRemove(Key, out _);
                }
            }
        }
    }
}
