using System;
using System.Threading;

namespace RestManagerLogic
{
    public class ReadWriteLockSlimDisposableWrap
    {
        /// <summary>
        /// Need to check additional info about not thread-abort safe environment and working with it as
        /// noted at https://docs.microsoft.com/en-us/dotnet/api/system.threading.readerwriterlockslim?view=net-5.0
        /// </summary>
        private readonly ReaderWriterLockSlim _readerWriterLock = new(LockRecursionPolicy.NoRecursion);

        public ReaderLock TakeReaderDisposableLock()
        {
            return new ReaderLock(this._readerWriterLock);
        }

        public WriterLock TakeWriterDisposableLock()
        {
            return new WriterLock(this._readerWriterLock);
        }

        public class ReaderLock: IDisposable
        {
            private readonly ReaderWriterLockSlim _readerWriterLock;

            public ReaderLock(ReaderWriterLockSlim readerWriterLock)
            {
                _readerWriterLock = readerWriterLock;

                _readerWriterLock.EnterReadLock();
            }
            
            public void Dispose()
            {
                _readerWriterLock.ExitReadLock();
            }
        }
        
        public class WriterLock: IDisposable
        {
            private readonly ReaderWriterLockSlim _readerWriterLock;
            
            public WriterLock(ReaderWriterLockSlim readerWriterLock)
            {
                _readerWriterLock = readerWriterLock;

                _readerWriterLock.EnterWriteLock();
            }
            
            public void Dispose()
            {
                _readerWriterLock.ExitWriteLock();
            }
        }
    }
}