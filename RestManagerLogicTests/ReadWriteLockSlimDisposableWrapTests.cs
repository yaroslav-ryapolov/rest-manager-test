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
        public void TestExecutionOrderSample()
        {
            var processingOrderQueue = new ConcurrentQueue<int>();
            var readerWriterLock = new ReadWriteLockSlimDisposableWrap();

            void ReaderAction(int number, int delay)
            {
                Console.WriteLine("Start - " + number);
                using (readerWriterLock.TakeReaderDisposableLock())
                {
                    Console.WriteLine("Taken - " + number);

                    Thread.Sleep(delay);
                    processingOrderQueue.Enqueue(number);
                }

                Console.WriteLine("End - " + number);
            }

            void WriterAction(int number, int delay)
            {
                Console.WriteLine("Start - " + number);
                using (readerWriterLock.TakeWriterDisposableLock())
                {
                    Console.WriteLine("Taken - " + number);

                    Thread.Sleep(delay);
                    processingOrderQueue.Enqueue(number);
                }

                Console.WriteLine("End - " + number);
            }

            var tasks = new List<Task>();

            Thread.Sleep(10);
            tasks.Add(Task.Run(() => ReaderAction(1, 200)));
            Thread.Sleep(10);
            tasks.Add(Task.Run(() => ReaderAction(2, 100)));
            
            Thread.Sleep(10);
            tasks.Add(Task.Run(() => WriterAction(3, 500)));

            Thread.Sleep(10);
            tasks.Add(Task.Run(() => ReaderAction(4, 100)));

            Thread.Sleep(200);
            tasks.Add(Task.Run(() => WriterAction(5, 300)));
            Thread.Sleep(10);
            tasks.Add(Task.Run(() => WriterAction(51, 300)));
            Thread.Sleep(10);
            tasks.Add(Task.Run(() => WriterAction(52, 300)));
            Thread.Sleep(10);
            tasks.Add(Task.Run(() => ReaderAction(6, 200)));
            Thread.Sleep(10);
            tasks.Add(Task.Run(() => ReaderAction(7, 100)));

            Task.WaitAll(tasks.ToArray());


            var processingOrder = processingOrderQueue.ToArray();
            Console.WriteLine(String.Join(',', processingOrder));
            
            Assert.Contains(2, processingOrder.Take(2).ToArray());
            Assert.Contains(1, processingOrder.Take(2).ToArray());
            Assert.AreEqual(3, processingOrder[2]);

            var writesOrder = new[] { 3, 5, 51, 52 };
            Assert.That(processingOrder.Where((v) => writesOrder.Contains(v)).AsEnumerable(), Is.EqualTo(writesOrder).AsCollection);
        }
    }
}