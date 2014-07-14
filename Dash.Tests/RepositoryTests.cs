using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using Dashboard;
using Dashboard.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dash.Tests
{
    [TestClass]
    public class RepositoryTests
    {
        private string userName;
        private string password;

        [TestInitialize]
        public void Setup()
        {
            userName = ConfigurationManager.AppSettings["username"];
            password = ConfigurationManager.AppSettings["password"];
        }

         [TestMethod, TestCategory("IntegrationTest")]
        public void ShouldGetWorkItemsFromQuery()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(ConfigurationManager.AppSettings["VSOnlineBaseUrl"]) })
            {
                var connection = new TfsConnection(userName, password, client);
                var repo = new WorkItemRepository(connection);
                var results = repo.GetInProcAndClosedWorkItems(@"BPS.Scrum\Dev -SEP Project");

                Assert.IsTrue(results.Result.Results.All(r => r.SourceId != 0));
            }
        }

        [TestMethod, TestCategory("IntegrationTest")]
        public void ShouldGetUpdatesForWorkItem()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(ConfigurationManager.AppSettings["VSOnlineBaseUrl"]) })
            {
                var connection = new TfsConnection(userName, password, client);
                var repo = new WorkItemRepository(connection);

                var results = repo.GetWorkItemUpdates(86);
                Assert.IsTrue(results.Result.All(s => s.Fields[0].Field != null));
                Assert.IsTrue(results.Result.All(s => s.Fields[0].UpdatedValue != null));
            }
        }

         [TestMethod, TestCategory("IntegrationTest")]
        public void ShouldGetworkItems()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(ConfigurationManager.AppSettings["VSOnlineBaseUrl"]) })
            {
                var connection = new TfsConnection(userName, password, client);
                var repo = new WorkItemRepository(connection);
                var results = repo.GetInProcAndClosedWorkItems(@"BPS.Scrum\Dev -SEP Project");
                var workItemIds = results.Result.Results.Select(r => r.SourceId).ToArray();

                var wi = repo.GetWorkItems(workItemIds);
                Assert.IsNotNull(wi.Result.First().Id);
                Assert.IsNotNull(wi.Result.First().Rev);
                Assert.IsNotNull(wi.Result.First().Title);
                Assert.IsNotNull(wi.Result.First().Effort);
                Assert.IsNotNull(wi.Result.First().State);
                Assert.IsNotNull(wi.Result.First().ChangedDate);
                Assert.IsNotNull(wi.Result.First().ClosedDate);
            }
        }
    }
}