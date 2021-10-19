using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RestManagerLogic;

namespace RestManagerLogicTests
{
    [TestFixture]
    public class RestManagerSimpleTests
    {
        private RestManagerSimple _restManagerSimple;

        [SetUp]
        public void Setup()
        {
            _restManagerSimple = new RestManagerSimple(new List<Table>
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
            
            _restManagerSimple.OnArrive(group);

            if (_restManagerSimple.Tables.Count(t => t.IsOccupied) != 1)
            {
                Assert.Fail("Wrong number of tables is occupied");
            }
            
            if (_restManagerSimple.Tables.Count((t) => t.SeatedClientGroups.Contains(group) && t.Size == 3) != 1)
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
            
            _restManagerSimple.OnArrive(group1);
            _restManagerSimple.OnArrive(group2);

            if (_restManagerSimple.Tables.Count(t => t.IsOccupied) != 2)
            {
                Assert.Fail("Wrong number of table is occupied");
            }
            
            if (_restManagerSimple.Tables.Count((t) => t.SeatedClientGroups.Contains(group1) && t.Size == 3) != 1)
            {
                Assert.Fail("Wrong table is occupied for group 1 (group should be seated to 3 persons table)");
            }
            
            if (_restManagerSimple.Tables.Count((t) => t.SeatedClientGroups.Contains(group2) && t.Size != 3) != 1)
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
            _restManagerSimple.OnArrive(group);
            _restManagerSimple.OnLeave(group);

            if (_restManagerSimple.Tables.Count(t => t.IsOccupied) != 0)
            {
                Assert.Fail("Wrong number of tables is occupied");
            }
            
            // second time to check release is really made table available
            _restManagerSimple.OnArrive(group);

            if (_restManagerSimple.Tables.Count(t => t.IsOccupied) != 1)
            {
                Assert.Fail("Wrong number of tables is occupied");
            }
            
            if (_restManagerSimple.Tables.Count((t) => t.SeatedClientGroups.Contains(group) && t.Size == 3) != 1)
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
            _restManagerSimple.OnArrive(group1_3); // 3rd taken
            _restManagerSimple.OnArrive(group2_3); // 4th taken
            _restManagerSimple.OnArrive(group3_3_control); // 5th taken
            _restManagerSimple.OnArrive(group4_3); // 6th taken
            
            _restManagerSimple.OnLeave(group3_3_control); // 5th is released
            _restManagerSimple.OnArrive(group3_3_control); // should take 5th table

            if (_restManagerSimple.Tables.Count((t) => t.SeatedClientGroups.Contains(group3_3_control) && t.Size == 5) != 1)
            {
                Assert.Fail("Wrong table is occupied (group should be seated at free 5 persons table)");
            }
            

            Assert.Pass();
        }
        
        [Test]
        public void NoPlacesWithExactButBiggerTablesAvailable()
        {
            _restManagerSimple = new RestManagerSimple(new List<Table>
            {
                new Table(6),
                new Table(6),
            });
            
            var group1 = new ClientsGroup(2);
            var group2 = new ClientsGroup(2);
            var group3 = new ClientsGroup(2);
            
            _restManagerSimple.OnArrive(group1);
            _restManagerSimple.OnArrive(group2);
            _restManagerSimple.OnArrive(group3);

            if (_restManagerSimple.Tables.Count((t) => t.SeatedClientGroups.Contains(group1)) != 1
                || _restManagerSimple.Tables.Count((t) => t.SeatedClientGroups.Contains(group2)) != 1
                || _restManagerSimple.Tables.Count((t) => t.SeatedClientGroups.Contains(group3)) != 1)
            {
                Assert.Fail("not all groups are seated");
            }

            if (_restManagerSimple.Tables.Count((t) => t.AvailableChairs == 2) != 1
                || _restManagerSimple.Tables.Count((t) => t.AvailableChairs == 4) != 1)
            {
                Assert.Fail("available seats number is not correct");
            }

            Assert.Pass();
        }

        [Test]
        public void PerformanceTest()
        {
            const int tablesCount = 1000;
            const int clientsCount = 10000;
            const int arrivalDelay = 10;
            const int leaveDelay = 30;

            var sizeRandomizer = new Random();

            var tables = new List<Table>(tablesCount);
            for (int i = 0; i < tablesCount; i++)
            {
                tables.Add(new Table(sizeRandomizer.Next(1, 7)));
            }
            
            var clients = new List<ClientsGroup>(clientsCount);
            for (int i = 0; i < clientsCount; i++)
            {
                clients.Add(new ClientsGroup(sizeRandomizer.Next(1, 7)));
            }

            var initializerStopwatch = new Stopwatch();
            initializerStopwatch.Start();
            _restManagerSimple = new RestManagerSimple(tables);
            initializerStopwatch.Stop();

            var arrivalTime = new TimeSpan(0);
            var arrivalTask = Task.Run(async () =>
            {
                var stopwatch = new Stopwatch();

                foreach (var group in clients)
                {
                    await Task.Delay(arrivalDelay);

                    stopwatch.Restart();
                    _restManagerSimple.OnArrive(group);
                    stopwatch.Stop();
                    arrivalTime += stopwatch.Elapsed;
                }
            });

            Thread.Sleep(1000);
            var leaveTime = new TimeSpan(0);
            var leaveTask = Task.Run(async () =>
            {
                var stopwatch = new Stopwatch();

                foreach (var group in clients)
                {
                    await Task.Delay(leaveDelay);

                    stopwatch.Restart();
                    _restManagerSimple.OnLeave(group);
                    stopwatch.Stop();
                    leaveTime += stopwatch.Elapsed;
                }
            });

            Task.WaitAll(arrivalTask, leaveTask);
            Assert.Pass($"Initialization [{initializerStopwatch.Elapsed:c}]; total rest-managing time [{(arrivalTime + leaveTime):c}];");
        }
    }
}