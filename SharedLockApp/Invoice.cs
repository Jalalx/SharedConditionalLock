using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLockApp
{

    public class Invoice : FinancialObject
    {
        public Invoice()
        {
            Type = FinancialType.Invoice;
        }
    }
}
