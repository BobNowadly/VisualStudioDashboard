using System;
using System.Configuration;
using System.Net.Http;
using Dashboard;
using Dashboard.DataAccess;

namespace WebApplication.App_Start
{
    internal class WorkItemRepositoryFactory : IDisposable
    {
        private readonly HttpClient client;

        public WorkItemRepositoryFactory()
        {
            client = new HttpClient {BaseAddress = new Uri(ConfigurationManager.AppSettings["VSOnlineBaseUrl"])};
        }
        public IWorkItemRepository CreateRepository()
        {
            return new WorkItemRepository(new TfsConnection(ConfigurationManager.AppSettings["username"],
                ConfigurationManager.AppSettings["password"], client));
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Dispose();
            }
        }
    }
}