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
            
            if (_restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group) && t.Size == 3) != 1)
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
            
            if (_restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group1) && t.Size == 3) != 1)
            {
                Assert.Fail("Wrong table is occupied for group 1 (group should be seated to 3 persons table)");
            }
            
            if (_restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group2) && t.Size != 3) != 1)
            {
                Assert.Fail("Wrong table is occupied for group 2 (group should be seated to 4 persons table)");
            }
            
            Assert.Pass();
        }
        
        [Test]
        public void ReleaseTable()
        {
            var group = new ClientsGroup(3);

            // first time to check release happened in general
            _restManager.OnArrive(group);
            _restManager.OnLeave(group);

            if (_restManager.Tables.Count(t => t.IsOccupied) != 0)
            {
                Assert.Fail("Wrong number of table is occupied");
            }
            
            // second time to check release is really made table available
            _restManager.OnArrive(group);

            if (_restManager.Tables.Count(t => t.IsOccupied) > 1)
            {
                Assert.Fail("More than one table is occupied");
            }
            
            if (_restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group) && t.Size == 3) != 1)
            {
                Assert.Fail("Wrong table is occupied (group should be seated to 3 persons table)");
            }
            
            Assert.Pass();
        }

        [Test]
        public void SeatFreeTablesAtFirst()
        {
            var group1_3 = new ClientsGroup(3);
            var group2_3 = new ClientsGroup(3);
            var group3_3_control = new ClientsGroup(3);
            var group4_3 = new ClientsGroup(3);

            // first time to check release happened in general
            _restManager.OnArrive(group1_3); // 3rd taken
            _restManager.OnArrive(group2_3); // 4th taken
            _restManager.OnArrive(group3_3_control); // 5th taken
            _restManager.OnArrive(group4_3); // 6th taken
            
            _restManager.OnLeave(group3_3_control); // 5th is released
            _restManager.OnArrive(group3_3_control); // should take 5th table

            if (_restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group3_3_control) && t.Size == 5) != 1)
            {
                Assert.Fail("Wrong table is occupied (group should be seated at free 5 persons table)");
            }
            

            Assert.Pass();
        }
    }
}