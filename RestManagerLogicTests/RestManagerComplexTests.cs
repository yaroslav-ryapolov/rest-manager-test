using System.Collections.Generic;
using NUnit.Framework;
using RestManagerLogic;
using RestManagerLogic.RestManagerLinkedList;

namespace RestManagerLogicTests
{
    [TestFixture]
    public class RestManagerComplexTests: RestManagerTestsBase
    {
        protected override IRestManager GetManager(List<Table> tables)
        {
            return new RestManagerComplex(tables);
        }

        [Test]
        public void SeatGroupAtTheTable()
        {
            SeatGroupAtTheTableBase();
        }

        [Test]
        public void Seat2GroupsSameSized()
        {
            Seat2GroupsSameSizedBase();
        }

        [Test]
        public void ReleaseTable()
        {
            ReleaseTableBase();
        }

        [Test]
        public void SeatFreeTablesAtFirst()
        {
            SeatFreeTablesAtFirstBase();
        }

        [Test]
        public void NoPlacesWithExactButBiggerTablesAvailable()
        {
            NoPlacesWithExactButBiggerTablesAvailableBase();
        }

        [Test]
        public void TestGoOutOfQueue()
        {
            TestGoOutOfQueueBase();
        }

        [Test]
        public void PerformanceTest()
        {
            PerformanceTestBase();
        }
    }
}
