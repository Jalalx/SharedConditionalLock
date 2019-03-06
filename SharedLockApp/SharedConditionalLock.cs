using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharedLockApp
{

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
