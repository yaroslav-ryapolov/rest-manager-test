using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RestManagerLogic;
using RestManagerLogic.RestManagerLinkedList;

namespace RestManagerLogicTests
{
    [TestFixture]
    public class RestManagerComplexTests
    {
        private RestManagerComplex _restManagerComplex;

        [SetUp]
        public void Setup()
        {
            _restManagerComplex = new RestManagerComplex(new List<Table>
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
            
            _restManagerComplex.OnArrive(group);

            if (_restManagerComplex.Tables.Count(t => t.IsOccupied) > 1)
            {
                Assert.Fail("More than one table is occupied");
            }
            
            if (_restManagerComplex.Tables.Count((t) => t.SeatedClientGroups.Contains(group) && t.Size == 3) != 1)
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
            
            _restManagerComplex.OnArrive(group1);
            _restManagerComplex.OnArrive(group2);

            if (_restManagerComplex.Tables.Count(t => t.IsOccupied) != 2)
            {
                Assert.Fail("Wrong number of table is occupied");
            }
            
            if (_restManagerComplex.Tables.Count((t) => t.SeatedClientGroups.Contains(group1) && t.Size == 3) != 1)
            {
                Assert.Fail("Wrong table is occupied for group 1 (group should be seated to 3 persons table)");
            }
            
            if (_restManagerComplex.Tables.Count((t) => t.SeatedClientGroups.Contains(group2) && t.Size != 3) != 1)
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
            _restManagerComplex.OnArrive(group);
            _restManagerComplex.OnLeave(group);

            if (_restManagerComplex.Tables.Count(t => t.IsOccupied) != 0)
            {
                Assert.Fail("Wrong number of tables is occupied");
            }
            
            // second time to check release is really made table available
            _restManagerComplex.OnArrive(group);

            if (_restManagerComplex.Tables.Count(t => t.IsOccupied) != 1)
            {
                Assert.Fail("Wrong number of tables is occupied");
            }
            
            if (_restManagerComplex.Tables.Count((t) => t.SeatedClientGroups.Contains(group) && t.Size == 3) != 1)
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
            _restManagerComplex.OnArrive(group1_3); // 3rd taken
            _restManagerComplex.OnArrive(group2_3); // 4th taken
            _restManagerComplex.OnArrive(group3_3_control); // 5th taken
            _restManagerComplex.OnArrive(group4_3); // 6th taken
            
            _restManagerComplex.OnLeave(group3_3_control); // 5th is released
            _restManagerComplex.OnArrive(group3_3_control); // should take 5th table

            if (_restManagerComplex.Tables.Count((t) => t.SeatedClientGroups.Contains(group3_3_control) && t.Size == 5) != 1)
            {
                Assert.Fail("Wrong table is occupied (group should be seated at free 5 persons table)");
            }
            

            Assert.Pass();
        }
        
        [Test]
        public void NoPlacesWithExactButBiggerTablesAvailable()
        {
            _restManagerComplex = new RestManagerComplex(new List<Table>
            {
                new Table(6),
                new Table(6),
            });
            
            var group1 = new ClientsGroup(2);
            var group2 = new ClientsGroup(2);
            var group3 = new ClientsGroup(2);
            
            _restManagerComplex.OnArrive(group1);
            _restManagerComplex.OnArrive(group2);
            _restManagerComplex.OnArrive(group3);

            if (_restManagerComplex.Tables.Count((t) => t.SeatedClientGroups.Contains(group1)) != 1
                || _restManagerComplex.Tables.Count((t) => t.SeatedClientGroups.Contains(group2)) != 1
                || _restManagerComplex.Tables.Count((t) => t.SeatedClientGroups.Contains(group3)) != 1)
            {
                Assert.Fail("not all groups are seated");
            }

            if (_restManagerComplex.Tables.Count((t) => t.AvailableChairs == 2) != 1
                || _restManagerComplex.Tables.Count((t) => t.AvailableChairs == 4) != 1)
            {
                Assert.Fail("available seats number is not correct");
            }

            Assert.Pass();
        }
    }
}