using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RestManagerLogic;

namespace RestManagerLogicTests
{
    public class RestManagerTests
    {
        private RestManager _restManager;

        [SetUp]
        public void Setup()
        {
            _restManager = new RestManager(new List<Table>
            {
                new Table(1),
                new Table(2),
                new Table(3),
                new Table(4),
                new Table(5),
                new Table(6),
            });
        }

        [Test]
        public void SeatGroupAtTheTable()
        {
            var group = new ClientsGroup(3);
            
            _restManager.OnArrive(group);

            if (_restManager.Tables.Count(t => t.IsOccupied) > 1)
            {
                Assert.Fail("More than one table is occupied");
            }
            
            if (_restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group) && t.Table.Size == 3) != 1)
            {
                Assert.Fail("Wrong table is occupied (group should be seated to 3 persons table)");
            }
            
            Assert.Pass();
        }
        
        [Test]
        public void Seat2GroupsSameSized()
        {
            var group1 = new ClientsGroup(3);
            var group2 = new ClientsGroup(3);
            
            _restManager.OnArrive(group1);
            _restManager.OnArrive(group2);

            if (_restManager.Tables.Count(t => t.IsOccupied) != 2)
            {
                Assert.Fail("Wrong number of table is occupied");
            }
            
            if (_restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group1) && t.Table.Size == 3) != 1)
            {
                Assert.Fail("Wrong table is occupied for group 1 (group should be seated to 3 persons table)");
            }
            
            if (_restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group2) && t.Table.Size != 3) != 1)
            {
                Assert.Fail("Wrong table is occupied for group 2 (group should be seated to 4 persons table)");
            }
            
            Assert.Pass();
        }
    }
}