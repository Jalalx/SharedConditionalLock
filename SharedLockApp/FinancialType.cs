using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLockApp
{

    public enum FinancialType
    {
        Invoice = 1,
        Payment = 2
    }
}
