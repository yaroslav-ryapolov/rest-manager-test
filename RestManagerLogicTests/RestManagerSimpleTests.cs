using System.Collections.Generic;
using NUnit.Framework;
using RestManagerLogic;

namespace RestManagerLogicTests
{
    [TestFixture]
    public class RestManagerSimpleTests: RestManagerTestsBase
    {
        protected override IRestManager GetManager(List<Table> tables)
        {
            return new RestManagerSimple(tables);
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
