using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLockApp
{

    public abstract class FinancialObject
    {
        public FinancialType Type { get; set; }

        public int Number { get; set; }
    }
}
