using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using Dashboard;
using Dashboard.Data;
using Dashboard.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dash.Tests
{
    [TestClass]
    public class HistorianTests
    {
        private Mock<IWorkItemRepository> mockRepo;

        [TestInitialize]
        public void SetUp()
        {
            mockRepo = new Mock<IWorkItemRepository>();
        }

        [TestMethod]
        public void ShoudldGetCommittedAndClosedWorkItems()
        {
            QueryResults inProcWorkItems = GetTestQueryResult();
            mockRepo.Setup(s => s.GetInProcAndClosedWorkItems(It.IsAny<string>())).ReturnsAsync(inProcWorkItems);
            IEnumerable<WorkItemUpdate> workitemUpdates = GetTestWorkItemUpdates();
            mockRepo.Setup(s => s.GetWorkItemUpdates(It.IsAny<int>())).ReturnsAsync(workitemUpdates);
            IEnumerable<WorkItemUpdate> workitems = GetTestWorkItems();
            mockRepo.Setup(s => s.GetWorkItems(It.IsAny<int[]>())).ReturnsAsync(workitems);

            var history = new Historian(mockRepo.Object);

            List<WorkItem> wi = history.GetCommittedAndClosedWorkItems();

            Assert.AreEqual(89, wi.First().Id);
            Assert.AreEqual("The title", wi.First().Title);
            Assert.AreEqual(8, wi.First().Effort);
            Assert.IsNotNull(wi.First().DateCommittedTime);
            Assert.IsNotNull(wi.First().DateClosed);
        }

        [TestMethod]
        public void ShouldHandleEarlyCloseCase()
        {
            QueryResults inProcWorkItems = GetTestQueryResult();
            mockRepo.Setup(s => s.GetInProcAndClosedWorkItems(It.IsAny<string>())).ReturnsAsync(inProcWorkItems);
            IEnumerable<WorkItemUpdate> workitemUpdates = GetTestWorkItemUpdateWithEarlyClose();
            mockRepo.Setup(s => s.GetWorkItemUpdates(It.IsAny<int>())).ReturnsAsync(workitemUpdates);
            IEnumerable<WorkItemUpdate> workitems = GetTestWorkItems();
            mockRepo.Setup(s => s.GetWorkItems(It.IsAny<int[]>())).ReturnsAsync(workitems);

            var history = new Historian(mockRepo.Object);

            List<WorkItem> wi = history.GetCommittedAndClosedWorkItems();

            Assert.AreEqual("7/8/2014", wi.First().DateClosed.Value.ToShortDateString());
        }

        [TestMethod]
        public void ShouldHandleAccidentalCloseCase()
        {
            QueryResults inProcWorkItems = GetTestQueryResult();
            mockRepo.Setup(s => s.GetInProcAndClosedWorkItems(It.IsAny<string>())).ReturnsAsync(inProcWorkItems);
            IEnumerable<WorkItemUpdate> workitemUpdates = GetTestWorkItemUpdateWithAccidentalClose();
            mockRepo.Setup(s => s.GetWorkItemUpdates(It.IsAny<int>())).ReturnsAsync(workitemUpdates);
            IEnumerable<WorkItemUpdate> workitems = GetTestWorkItems();
            mockRepo.Setup(s => s.GetWorkItems(It.IsAny<int[]>())).ReturnsAsync(workitems);

            var history = new Historian(mockRepo.Object);

            List<WorkItem> wi = history.GetCommittedAndClosedWorkItems();

            Assert.IsFalse(wi.First().DateClosed.HasValue);
        }

        [TestMethod]
        public void ShouldHandleEarlyCommittedCase()
        {
            QueryResults inProcWorkItems = GetTestQueryResult();
            mockRepo.Setup(s => s.GetInProcAndClosedWorkItems(It.IsAny<string>())).ReturnsAsync(inProcWorkItems);
            IEnumerable<WorkItemUpdate> workitemUpdates = GetTestWorkItemUpdateWithEarlyClose();
            mockRepo.Setup(s => s.GetWorkItemUpdates(It.IsAny<int>())).ReturnsAsync(workitemUpdates);
            IEnumerable<WorkItemUpdate> workitems = GetTestWorkItems();
            mockRepo.Setup(s => s.GetWorkItems(It.IsAny<int[]>())).ReturnsAsync(workitems);

            var history = new Historian(mockRepo.Object);

            List<WorkItem> wi = history.GetCommittedAndClosedWorkItems();

            Assert.AreEqual("7/6/2014", wi.First().DateCommittedTime.Value.ToShortDateString());
        }

        [TestMethod]
        public void ShouldHandleAccidentalCommittedCase()
        {
            QueryResults inProcWorkItems = GetTestQueryResult();
            mockRepo.Setup(s => s.GetInProcAndClosedWorkItems(It.IsAny<string>())).ReturnsAsync(inProcWorkItems);
            IEnumerable<WorkItemUpdate> workitemUpdates = GetTestWorkItemUpdateWithAccidentalCommit();
            mockRepo.Setup(s => s.GetWorkItemUpdates(It.IsAny<int>())).ReturnsAsync(workitemUpdates);
            IEnumerable<WorkItemUpdate> workitems = GetTestWorkItems();
            mockRepo.Setup(s => s.GetWorkItems(It.IsAny<int[]>())).ReturnsAsync(workitems);

            var history = new Historian(mockRepo.Object);

            List<WorkItem> wi = history.GetCommittedAndClosedWorkItems();

            Assert.IsFalse(wi.First().DateCommittedTime.HasValue);
        }

        [TestMethod]
        public void ShoudldGetCommittedAndClosedWorkItemsAsOfACertianDate()
        {
            QueryResults allResults = GetTestQueryResult();
            mockRepo.Setup(s => s.GetPrdouctBacklogItemsAsOf(It.IsAny<string>(), It.IsAny<DateTime>(), It.Is<string>(a => a == null))).ReturnsAsync(allResults);
            QueryResults closedResults = GetDoneQueryResult();
            mockRepo.Setup(s => s.GetPrdouctBacklogItemsAsOf(It.IsAny<string>(), It.IsAny<DateTime>(), It.Is<string>(a => a == "Done"))).ReturnsAsync(closedResults);
           
            IEnumerable<WorkItemUpdate> workitems = GetTestWorkItems();
            mockRepo.Setup(s => s.GetWorkItemsAsOf(It.IsAny<DateTime>(), It.Is<int[]>(a => a.Length == 2))).ReturnsAsync(workitems);
            mockRepo.Setup(s => s.GetWorkItemsAsOf(It.IsAny<DateTime>(), It.Is<int[]>(a => a.Length == 1))).ReturnsAsync(workitems.Where(s => s.State == "Done"));

            var history = new Historian(mockRepo.Object);

            var burnUpData = history.GetBurnUpDataSince(new DateTime(2014, 7, 31), "Test area");

            Assert.AreEqual(11, burnUpData.Requested.First().Count);
            Assert.AreEqual(3, burnUpData.Completed.First().Count);
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void ShoudldGetCBurnupAsOfACertianDateIntegrationTest()
        {
            using (
                var client = new HttpClient {BaseAddress = new Uri(ConfigurationManager.AppSettings["VSOnlineBaseUrl"])}
                )
            {
                var history = new Historian(
                    new WorkItemRepository(new TfsConnection(ConfigurationManager.AppSettings["username"],
                        ConfigurationManager.AppSettings["password"], client)));

                var burnup = history.GetBurnUpDataSince(new DateTime(2014, 7, 31, 23, 59, 59), @"BPS.Scrum\Dev -SEP Project");

                Assert.AreEqual(213, burnup.Requested.First().Count);
                Assert.AreEqual(38, burnup.Completed.First().Count);
            }
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void ShouldHaveZerosForDatesThatDoNotHaveCompletedPRBI()
        {
            using (
                var client = new HttpClient { BaseAddress = new Uri(ConfigurationManager.AppSettings["VSOnlineBaseUrl"]) }
                )
            {
                var history = new Historian(
                    new WorkItemRepository(new TfsConnection(ConfigurationManager.AppSettings["username"],
                        ConfigurationManager.AppSettings["password"], client)));
                var firstDate = new DateTime(2014, 7, 7, 23, 59, 59);
                var burnup = history.GetBurnUpDataSince(firstDate, @"BPS.Scrum\Dev -SEP Project");

                Assert.AreEqual(firstDate.Date, burnup.Completed.First().Date.Date);
                Assert.AreEqual(0, burnup.Completed.First().Count);
            }
        }

        private IEnumerable<WorkItemUpdate> GetTestWorkItems()
        {
            return new List<WorkItemUpdate>
            {
                new WorkItemUpdate
                {
                    Id = 89,
                    Fields = new List<WorkItemFieldValue>
                    {
                        new WorkItemFieldValue
                        {
                            Value = "The title",
                            Field = new WorkItemField {Name = "Title"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "8",
                            Field = new WorkItemField {Name = "Effort"}
                        }
                    }
                },
                new WorkItemUpdate
                {
                    Id = 99,
                    Fields = new List<WorkItemFieldValue>
                    {
                        new WorkItemFieldValue
                        {
                            Value = "The title",
                            Field = new WorkItemField {Name = "Title"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "Done",
                            Field = new WorkItemField {Name = "State"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "3",
                            Field = new WorkItemField {Name = "Effort"}
                        }
                    }
                }
            };
        }

        private IEnumerable<WorkItemUpdate> GetTestWorkItemUpdates()
        {
            return new List<WorkItemUpdate>
            {
                new WorkItemUpdate
                {
                    Fields = new List<WorkItemFieldValue>
                    {
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 4).ToShortDateString(),
                            Field = new WorkItemField {Name = "Changed Date"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "Committed",
                            Field = new WorkItemField {Name = "State"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "8",
                            Field = new WorkItemField {Name = "Effort"}
                        }
                    }
                },
                new WorkItemUpdate
                {
                    Fields = new List<WorkItemFieldValue>
                    {
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 8).ToShortDateString(),
                            Field = new WorkItemField {Name = "Closed Date"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "Done",
                            Field = new WorkItemField {Name = "State"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 8).ToShortDateString(),
                            Field = new WorkItemField {Name = "Changed Date"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "3",
                            Field = new WorkItemField {Name = "Effort"}
                        }
                    }
                }
            };
        }

        private QueryResults GetTestQueryResult()
        {
            return new QueryResults
            {
                Results = new[]
                {
                    new QueryResult {SourceId = 88},
                    new QueryResult {SourceId = 89}
                }
            };
        }

        private QueryResults GetDoneQueryResult()
        {
            return new QueryResults
            {
                Results = new[]
                {
                    new QueryResult {SourceId = 88}
                }
            };
        }

        private IEnumerable<WorkItemUpdate> GetTestWorkItemUpdateWithEarlyClose()
        {
            return new List<WorkItemUpdate>
            {
                new WorkItemUpdate
                {
                    Fields = new List<WorkItemFieldValue>
                    {
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 4).ToShortDateString(),
                            Field = new WorkItemField {Name = "Changed Date"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "Committed",
                            Field = new WorkItemField {Name = "State"}
                        }
                    }
                },
                new WorkItemUpdate
                {
                    Fields = new List<WorkItemFieldValue>
                    {
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 5).ToShortDateString(),
                            Field = new WorkItemField {Name = "Changed Date"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "New",
                            Field = new WorkItemField {Name = "State"}
                        }
                    }
                },
                new WorkItemUpdate
                {
                    Fields = new List<WorkItemFieldValue>
                    {
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 6).ToShortDateString(),
                            Field = new WorkItemField {Name = "Changed Date"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "Committed",
                            Field = new WorkItemField {Name = "State"}
                        }
                    }
                },
                new WorkItemUpdate
                {
                    Fields = new List<WorkItemFieldValue>
                    {
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 8).ToShortDateString(),
                            Field = new WorkItemField {Name = "Closed Date"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "Done",
                            Field = new WorkItemField {Name = "State"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 8).ToShortDateString(),
                            Field = new WorkItemField {Name = "Changed Date"}
                        }
                    }
                },
                new WorkItemUpdate
                {
                    Fields = new List<WorkItemFieldValue>
                    {
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 3).ToShortDateString(),
                            Field = new WorkItemField {Name = "Closed Date"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "Done",
                            Field = new WorkItemField {Name = "State"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 3).ToShortDateString(),
                            Field = new WorkItemField {Name = "Changed Date"}
                        }
                    }
                }
            };
        }

        private IEnumerable<WorkItemUpdate> GetTestWorkItemUpdateWithAccidentalClose()
        {
            return new List<WorkItemUpdate>
            {
                new WorkItemUpdate
                {
                    Fields = new List<WorkItemFieldValue>
                    {
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 4).ToShortDateString(),
                            Field = new WorkItemField {Name = "Changed Date"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "Committed",
                            Field = new WorkItemField {Name = "State"}
                        }
                    }
                },
                new WorkItemUpdate
                {
                    Fields = new List<WorkItemFieldValue>
                    {
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 3).ToShortDateString(),
                            Field = new WorkItemField {Name = "Closed Date"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "Done",
                            Field = new WorkItemField {Name = "State"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 3).ToShortDateString(),
                            Field = new WorkItemField {Name = "Changed Date"}
                        }
                    }
                }
            };
        }

        private IEnumerable<WorkItemUpdate> GetTestWorkItemUpdateWithAccidentalCommit()
        {
            return new List<WorkItemUpdate>
            {
                new WorkItemUpdate
                {
                    Fields = new List<WorkItemFieldValue>
                    {
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 4).ToShortDateString(),
                            Field = new WorkItemField {Name = "Changed Date"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = "Committed",
                            Field = new WorkItemField {Name = "State"}
                        }
                    }
                },
                new WorkItemUpdate
                {
                    Fields = new List<WorkItemFieldValue>
                    {
                        new WorkItemFieldValue
                        {
                            Value = "New",
                            Field = new WorkItemField {Name = "State"}
                        },
                        new WorkItemFieldValue
                        {
                            Value = new DateTime(2014, 7, 5).ToShortDateString(),
                            Field = new WorkItemField {Name = "Changed Date"}
                        }
                    }
                }
            };
        }
    }
}