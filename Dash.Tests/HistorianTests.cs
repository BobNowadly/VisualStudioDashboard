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
        public void ShoudldGetCommittedAndClosedWorkItemsAsOfACertianDate()
        {
            QueryResults allResults = GetTestQueryResult();
            mockRepo.Setup(s => s.GetPrdouctBacklogItemsAsOf(It.IsAny<string>(), It.IsAny<DateTime>(), It.Is<string>(a => a == null), It.Is<string>(a => a == null))).ReturnsAsync(allResults);
            QueryResults closedResults = GetDoneQueryResult();
            mockRepo.Setup(s => s.GetPrdouctBacklogItemsAsOf(It.IsAny<string>(), It.IsAny<DateTime>(), It.Is<string>(a => a == "Done"), It.Is<string>(a => a == null))).ReturnsAsync(closedResults);
           
            var workitems = GetTestWorkItems();
            mockRepo.Setup(s => s.GetWorkItemsAsOf(It.IsAny<DateTime>(), It.Is<int[]>(a => a.Length == 2))).ReturnsAsync(workitems);
            mockRepo.Setup(s => s.GetWorkItemsAsOf(It.IsAny<DateTime>(), It.Is<int[]>(a => a.Length == 1))).ReturnsAsync(workitems.Where(s => s.State == "Done"));

            var history = new Historian(mockRepo.Object);

            var burnUpData = history.GetBurnUpDataSince(new DateTime(2014, 7, 30), "Test area");

            Assert.AreEqual(11, burnUpData.First(s => s.Title == "Requested").Data.First().Value);
            Assert.AreEqual(3, burnUpData.First(s => s.Title == "Completed").Data.First().Value);          
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

                var burnup = history.GetBurnUpDataSince(new DateTime(2014, 7, 30, 23, 59, 59), @"BPS.Scrum\Dev -SEP Project");

                Assert.AreEqual(213, (int)burnup.First(s => s.Title == "Requested").Data.First().Value);
                Assert.AreEqual(38, (int)burnup.First(s => s.Title == "Completed").Data.First().Value);            
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

                Assert.AreEqual(firstDate.Date.Date, burnup.First(s => s.Title == "Completed").Data.First().Date.Date);
                Assert.AreEqual(0, (int)burnup.First(s => s.Title == "Completed").Data.First().Value );               
            }
        }

        private IEnumerable<WorkItemUpdateBase> GetTestWorkItems()
        {
            return new List<WorkItemUpdateBase>
            {
                new WorkItemJson
                {
                    Id = 89,
                    Fields = new Dictionary<string, string>()
                    {
                        {"System.Title", "Title"},
                        {"Microsoft.VSTS.Scheduling.Effort", "8"}

                    }
                },
                new WorkItemJson
                {
                    Id = 99,
                    Fields = new Dictionary<string, string>()
                    {
                        {"System.Title", "The title"},
                        {"System.State", "Done"},
                        {"Microsoft.VSTS.Scheduling.Effort", "3"}

                    }
                }
            };
        }

        private QueryResults GetTestQueryResult()
        {
            return new QueryResults
            {
                WorkItems = new[]
                {
                    new QueryResult {Id = 88},
                    new QueryResult {Id = 89}
                }
            };
        }

        private QueryResults GetDoneQueryResult()
        {
            return new QueryResults
            {
                WorkItems = new[]
                {
                    new QueryResult {Id = 88}
                }
            };
        }
    }
}