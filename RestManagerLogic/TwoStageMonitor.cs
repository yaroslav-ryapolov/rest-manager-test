using System;
using System.Threading;

namespace RestManagerLogic
{
    public class TwoStageMonitor
    {
        private int _activeCounter;
        private readonly AutoResetEvent _counterChangeEvent = new(false);
        private readonly ManualResetEventSlim _counterAllowEvent = new(true);
        private readonly object _allSlotsLock = new();

        // todo: introduce time limit param
        public OneSlot TakeOne()
        {
            return new OneSlot(this);
        }

        // todo: introduce time limit param
        public AllSlots TakeAll()
        {
            return new AllSlots(this);
        }

        public class OneSlot: IDisposable
        {
            private readonly TwoStageMonitor _monitor;
            
            public OneSlot(TwoStageMonitor monitor)
            {
                _monitor = monitor;

                _monitor._counterAllowEvent.Wait();
                
                // >> беда если читатель тут #1
                
                Interlocked.Increment(ref _monitor._activeCounter);
                _monitor._counterChangeEvent.Set();
            }
            
            public void Dispose()
            {
                Interlocked.Decrement(ref _monitor._activeCounter);
                _monitor._counterChangeEvent.Set();
            }
        }
        
        public class AllSlots: IDisposable
        {
            private readonly TwoStageMonitor _monitor;
            
            public AllSlots(TwoStageMonitor monitor)
            {
                _monitor = monitor;

                lock (_monitor._allSlotsLock)
                {
                    _monitor._counterAllowEvent.Wait();
                    _monitor._counterAllowEvent.Reset();
                }

                // >> беда если писатель тут #1

                while (Volatile.Read(ref _monitor._activeCounter) > 0)
                {
                    _monitor._counterChangeEvent.WaitOne();
                }
            }
            
            public void Dispose()
            {
                _monitor._counterAllowEvent.Set();
            }
        }
    }
}