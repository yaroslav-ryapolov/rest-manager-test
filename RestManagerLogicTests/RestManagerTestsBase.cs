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
    public abstract class RestManagerTestsBase
    {
        protected abstract IRestManager GetManager(List<Table> tables);

        private IRestManager GetManager(params int[] tableSizes)
        {
            return GetManager(tableSizes.Select((s) => new Table(s)).ToList());
        }

        protected void SeatGroupAtTheTableBase()
        {
            var restManager = GetManager(3, 3);
            var group = new ClientsGroup(3);

            restManager.OnArrive(group);

            if (restManager.Tables.Count(t => t.IsOccupied) != 1)
            {
                Assert.Fail("Wrong number of tables is occupied");
            }

            if (restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group) && t.Size == 3) != 1)
            {
                Assert.Fail("Wrong table is occupied (group should be seated to 3 persons table)");
            }

            Assert.Pass();
        }

        protected void Seat2GroupsSameSizedBase()
        {
            var restManager = GetManager(1, 2, 3, 4, 5, 6);
            var group1 = new ClientsGroup(3);
            var group2 = new ClientsGroup(3);

            restManager.OnArrive(group1);
            restManager.OnArrive(group2);

            if (restManager.Tables.Count(t => t.IsOccupied) != 2)
            {
                Assert.Fail("Wrong number of tables is occupied");
            }

            if (restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group1) && t.Size == 3) != 1)
            {
                Assert.Fail("Wrong table is occupied for group 1 (group should be seated to 3 persons table)");
            }

            if (restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group2) && t.Size != 3) != 1)
            {
                Assert.Fail("Wrong table is occupied for group 2 (group should be seated to 4 persons table)");
            }

            Assert.Pass();
        }

        protected void ReleaseTableBase()
        {
            var restManager = GetManager(1, 2, 3, 4, 5, 6);
            var group = new ClientsGroup(3);

            // first time to check release happened in general
            restManager.OnArrive(group);
            restManager.OnLeave(group);

            if (restManager.Tables.Count(t => t.IsOccupied) != 0)
            {
                Assert.Fail("Wrong number of tables is occupied");
            }

            // second time to check release is really made table available
            restManager.OnArrive(group);

            if (restManager.Tables.Count(t => t.IsOccupied) != 1)
            {
                Assert.Fail("Wrong number of tables is occupied");
            }

            if (restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group) && t.Size == 3) != 1)
            {
                Assert.Fail("Wrong table is occupied (group should be seated to 3 persons table)");
            }

            Assert.Pass();
        }

        protected void SeatFreeTablesAtFirstBase()
        {
            var restManager = GetManager(1, 2, 3, 4, 5, 6);

            var group1_3 = new ClientsGroup(3);
            var group2_3 = new ClientsGroup(3);
            var group3_3_control = new ClientsGroup(3);
            var group4_3 = new ClientsGroup(3);

            // first time to check release happened in general
            restManager.OnArrive(group1_3); // 3rd taken
            restManager.OnArrive(group2_3); // 4th taken
            restManager.OnArrive(group3_3_control); // 5th taken
            restManager.OnArrive(group4_3); // 6th taken

            restManager.OnLeave(group3_3_control); // 5th is released
            restManager.OnArrive(group3_3_control); // should take 5th table

            if (restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group3_3_control) && t.Size == 5) != 1)
            {
                Assert.Fail("Wrong table is occupied (group should be seated at free 5 persons table)");
            }

            Assert.Pass();
        }

        protected void NoPlacesWithExactButBiggerTablesAvailableBase()
        {
            var restManager = GetManager(6, 6);

            var group1 = new ClientsGroup(2);
            var group2 = new ClientsGroup(2);
            var group3 = new ClientsGroup(2);

            restManager.OnArrive(group1);
            restManager.OnArrive(group2);
            restManager.OnArrive(group3);

            if (restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group1)) != 1
                || restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group2)) != 1
                || restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group3)) != 1)
            {
                Assert.Fail("not all groups are seated");
            }

            if (restManager.Tables.Count((t) => t.AvailableChairs == 2) != 1
                || restManager.Tables.Count((t) => t.AvailableChairs == 4) != 1)
            {
                Assert.Fail("available seats number is not correct");
            }

            Assert.Pass();
        }

        protected void TestGoOutOfQueueBase()
        {
            var restManager = GetManager(6, 6);

            var group1 = new ClientsGroup(5);
            var group2 = new ClientsGroup(5);
            var group3 = new ClientsGroup(5);
            var group4 = new ClientsGroup(5);

            restManager.OnArrive(group1);
            restManager.OnArrive(group2);
            restManager.OnArrive(group3);
            restManager.OnArrive(group4);

            if (restManager.Tables.Any((t) => t.SeatedClientGroups.Contains(group3))
                || restManager.Tables.Any((t) => t.SeatedClientGroups.Contains(group4)))
            {
                Assert.Fail("group3 and group4 should wait in queue");
            }

            restManager.OnLeave(group1);
            if (restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group3)) != 1)
            {
                Assert.Fail("group3 should be seated");
            }

            restManager.OnLeave(group2);
            if (restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group3)) != 1)
            {
                Assert.Fail("group3 should be seated at one table only");
            }
            if (restManager.Tables.Count((t) => t.SeatedClientGroups.Contains(group4)) != 1)
            {
                Assert.Fail("group4 should be seated");
            }

            Assert.Pass();
        }

        protected void PerformanceTestBase()
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
            var restManager = GetManager(tables);
            initializerStopwatch.Stop();

            var arrivalTime = new TimeSpan(0);
            var arrivalTask = Task.Run(async () =>
            {
                var stopwatch = new Stopwatch();

                foreach (var group in clients)
                {
                    await Task.Delay(arrivalDelay);

                    stopwatch.Restart();
                    restManager.OnArrive(group);
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
                    restManager.OnLeave(group);
                    stopwatch.Stop();
                    leaveTime += stopwatch.Elapsed;
                }
            });

            Task.WaitAll(arrivalTask, leaveTask);
            Assert.Pass($"Initialization [{initializerStopwatch.Elapsed:c}]; total rest-managing time [{(arrivalTime + leaveTime):c}];");
        }
    }
}
