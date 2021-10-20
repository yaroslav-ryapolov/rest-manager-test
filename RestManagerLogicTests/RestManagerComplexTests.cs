using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public void Seat2GroupsSameSized()
        {
            var group1 = new ClientsGroup(3);
            var group2 = new ClientsGroup(3);
            
            _restManagerComplex.OnArrive(group1);
            _restManagerComplex.OnArrive(group2);

            if (_restManagerComplex.Tables.Count(t => t.IsOccupied) != 2)
            {
                Assert.Fail("Wrong number of tables is occupied");
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
        
        [Test]
        public void TestGoOutOfQueue()
        {
            _restManagerComplex = new RestManagerComplex(new List<Table>
            {
                new Table(6),
                new Table(6),
            });

            var group1 = new ClientsGroup(5);
            var group2 = new ClientsGroup(5);
            var group3 = new ClientsGroup(5);
            var group4 = new ClientsGroup(5);
            
            _restManagerComplex.OnArrive(group1);
            _restManagerComplex.OnArrive(group2);
            _restManagerComplex.OnArrive(group3);
            _restManagerComplex.OnArrive(group4);

            if (_restManagerComplex.Tables.Any((t) => t.SeatedClientGroups.Contains(group3))
                || _restManagerComplex.Tables.Any((t) => t.SeatedClientGroups.Contains(group4)))
            {
                Assert.Fail("group3 and group4 should wait in queue");
            }

            _restManagerComplex.OnLeave(group1);
            if (_restManagerComplex.Tables.Count((t) => t.SeatedClientGroups.Contains(group3)) != 1)
            {
                Assert.Fail("group3 should be seated");
            }
            
            _restManagerComplex.OnLeave(group2);
            if (_restManagerComplex.Tables.Count((t) => t.SeatedClientGroups.Contains(group3)) != 1)
            {
                Assert.Fail("group3 should be seated at one table only");
            }
            if (_restManagerComplex.Tables.Count((t) => t.SeatedClientGroups.Contains(group4)) != 1)
            {
                Assert.Fail("group4 should be seated");
            }
            
            Assert.Pass();
        }

        [Test]
        public void PerformanceTest()
        {
            const int tablesCount = 1000;
            const int clientsCount = 10000;
            const int arrivalDelay = 1;
            const int leaveDelay = 3;

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
            _restManagerComplex = new RestManagerComplex(tables);
            initializerStopwatch.Stop();

            var arrivalTime = new TimeSpan(0);
            var arrivalTask = Task.Run(async () =>
            {
                var stopwatch = new Stopwatch();

                foreach (var group in clients)
                {
                    await Task.Delay(arrivalDelay);

                    stopwatch.Restart();
                    _restManagerComplex.OnArrive(group);
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
                    _restManagerComplex.OnLeave(group);
                    stopwatch.Stop();
                    leaveTime += stopwatch.Elapsed;
                }
            });

            Task.WaitAll(arrivalTask, leaveTask);
            Assert.Pass($"Initialization [{initializerStopwatch.Elapsed:c}]; total rest-managing time [{(arrivalTime + leaveTime):c}];");
        }
    }
}