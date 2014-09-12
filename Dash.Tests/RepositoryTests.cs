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

                Assert.IsTrue(results.Result.WorkItems.All(r => r.Id != 0));
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
                Assert.IsTrue(results.Result.Where(s => s.ChangedDate != string.Empty).All(s => s.Id != 0));
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
                var workItemIds = results.Result.WorkItems.Select(r => r.Id).ToArray();

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

         [TestMethod, TestCategory("IntegrationTest")]
         public void ShouldGetworkItemsAsOf()
         {
             using (var client = new HttpClient() { BaseAddress = new Uri(ConfigurationManager.AppSettings["VSOnlineBaseUrl"]) })
             {
                 var connection = new TfsConnection(userName, password, client);
                 var repo = new WorkItemRepository(connection);

                 var results = repo.GetPrdouctBacklogItemsAsOf(@"BPS.Scrum\Dev -SEP Project", DateTime.Now.AddDays(-10));
                 var workItemIds = results.Result.WorkItems.Select(r => r.Id).ToArray();


                 var wi = repo.GetWorkItemsAsOf(DateTime.Now.AddDays(-10), workItemIds);
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