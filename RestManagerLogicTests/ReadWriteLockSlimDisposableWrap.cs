using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RestManagerLogic;

namespace RestManagerLogicTests
{
    [TestFixture]
    public class ReadWriteLockSlimDisposableWrapTests
    {
        [Test]
        public void TestIt2()
        {
            ConcurrentQueue<int> processingOrder = new ConcurrentQueue<int>();
            var readerWriterLock = new ReadWriteLockSlimDisposableWrap();

            Action<int, int> readerAction = (int number, int delay) =>
            {
                Console.WriteLine("Start - " + number);
                using (readerWriterLock.TakeReaderDisposableLock())
                {
                    Console.WriteLine("Taken - " + number);
                    
                    Thread.Sleep(delay);
                    processingOrder.Enqueue(number);
                }
                Console.WriteLine("End - " + number);
            };
            Action<int, int> writerAction = (int number, int delay) =>
            {
                Console.WriteLine("Start - " + number);
                using (readerWriterLock.TakeWriterDisposableLock())
                {
                    Console.WriteLine("Taken - " + number);
                    
                    Thread.Sleep(delay);
                    processingOrder.Enqueue(number);
                }
                Console.WriteLine("End - " + number);
            };

            var tasks = new List<Task>();

            Thread.Sleep(10);
            tasks.Add(Task.Run(() => readerAction(1, 200)));
            Thread.Sleep(10);
            tasks.Add(Task.Run(() => readerAction(2, 100)));
            
            Thread.Sleep(10);
            tasks.Add(Task.Run(() => writerAction(3, 500)));

            Thread.Sleep(10);
            tasks.Add(Task.Run(() => readerAction(4, 100)));

            Thread.Sleep(200);
            tasks.Add(Task.Run(() => writerAction(5, 300)));
            Thread.Sleep(10);
            tasks.Add(Task.Run(() => writerAction(51, 300)));
            Thread.Sleep(10);
            tasks.Add(Task.Run(() => writerAction(52, 300)));
            Thread.Sleep(10);
            tasks.Add(Task.Run(() => readerAction(6, 200)));
            Thread.Sleep(10);
            tasks.Add(Task.Run(() => readerAction(7, 100)));

            Task.WaitAll(tasks.ToArray());

            // int finished = 0;
            // while (finished < 7)
            // {
            //     Task.WaitAny(tasks.ToArray());
            //     finished++;
            // }

            Console.WriteLine(String.Join(',', processingOrder.ToArray()));
            
            Assert.That(processingOrder.AsEnumerable(), Is.EqualTo(new[] {2, 1, 3, 4, 5, 51, 52, 7, 6}).AsCollection);
        }

        private static int Deque(ConcurrentQueue<int> queue)
        {
            queue.TryDequeue(out var order);
            return order;
        }
    }
}