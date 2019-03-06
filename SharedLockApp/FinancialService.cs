using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLockApp
{

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
}
