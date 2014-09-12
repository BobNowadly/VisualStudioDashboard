using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dashboard.Data;
using Newtonsoft.Json;

namespace Dashboard.DataAccess
{
    public interface IWorkItemRepository
    {
        Task<QueryResults> GetInProcAndClosedWorkItems(string area);
        Task<IEnumerable<WorkItemUpdateBase>> GetWorkItemUpdates(int workItemId);
        Task<IEnumerable<WorkItemUpdateBase>> GetWorkItems(params int[] ids);
        Task<IEnumerable<WorkItemUpdateBase>> GetWorkItemsAsOf(DateTime asOf, params int[] ids);
        Task<QueryResults> GetPrdouctBacklogItemsAsOf(string area, DateTime asOfDate, string state = null, string workitemType = null);
    }

    public class WorkItemRepository : IWorkItemRepository
    {
        private readonly TfsConnection connection;

        public WorkItemRepository(TfsConnection connection)
        {
            this.connection = connection;
        }

        public async Task<QueryResults> GetInProcAndClosedWorkItems(string area)
        {
            string query = string.Format(@"Select [System.Id], [System.Title], [System.State] From WorkItems Where [System.WorkItemType] = 'Product Backlog Item' AND [State] <> 'Removed' AND [State] EVER 'Closed' or [State] EVER 'Committed' AND [Area Path] Under  '{0}' order by [Microsoft.VSTS.Common.Priority] asc, [System.CreatedDate] desc", area);

            using (Task<HttpResponseMessage> response = connection.PostAsync("BPS.Scrum/_apis/wit/wiql?&api-version=1.0-preview.2",
                new KeyValuePair<string, string>("query", query)))
            {
                var workItems =
                    JsonConvert.DeserializeObject<QueryResults>(await response.Result.Content.ReadAsStringAsync());

                return workItems;
            }
        }

        public async Task<IEnumerable<WorkItemUpdateBase>> GetWorkItemUpdates(int workItemId)
        {
            string url = string.Format("_apis/wit/workitems/{0}/updates?&api-version=1.0-preview.2", workItemId);
            using (Task<HttpResponseMessage> response = connection.GetAsync(url))
            {
                var updates =
                    JsonConvert.DeserializeObject<WorkItemUpdates>(
                        await response.Result.Content.ReadAsStringAsync());

                return updates.Value;
            }
        }

        public async Task<IEnumerable<WorkItemUpdateBase>> GetWorkItems(params int[] ids)
        {
            string idString = string.Join(",", ids);
            string url =
                string.Format("_apis/wit/workitems?ids={0}&api-version=1.0-preview",
                    idString);
            using (Task<HttpResponseMessage> response = connection.GetAsync(url))
            {
                var workItems =
                    JsonConvert.DeserializeObject<WorkItemsJson>(
                        await response.Result.Content.ReadAsStringAsync());

                return workItems.Value;
            }
        }

        public async Task<IEnumerable<WorkItemUpdateBase>> GetWorkItemsAsOf(DateTime asOf, params int[] ids)
        {
            string idString = string.Join(",", ids);
            const string fields = "system.title,Microsoft.VSTS.Scheduling.Effort,System.State,System.ChangedDate,Microsoft.VSTS.Common.ClosedDate";

            string url =
                string.Format("_apis/wit/workitems?ids={0}&fields={2}&asOf={1}&api-version=1.0-preview",
                    idString, asOf.ToString("yyyy-MM-dd"), fields);
            using (Task<HttpResponseMessage> response = connection.GetAsync(url))
            {
                var workItems =
                    JsonConvert.DeserializeObject<WorkItemsJson>(
                        await response.Result.Content.ReadAsStringAsync());

                return workItems.Value;
            }
        }

        public async Task<QueryResults> GetPrdouctBacklogItemsAsOf(string area, DateTime asOfDate, string state = null, string workitemType = null)
        {
            var stateString = state != null ? "AND [State] = '" + state + "'" : string.Empty;
            var wit = workitemType ?? "Product Backlog Item";

            string query = string.Format(@"Select [System.Id], [System.Title], [System.State], [Microsoft.VSTS.Scheduling.Effort]
                        From WorkItems 
                        Where [System.WorkItemType] = '{2}'
                        AND [State] <> 'Removed'" 
                        + stateString + 
                        @"AND [Area Path] Under '{0}'
                        asof '{1}'
                        order by [Microsoft.VSTS.Common.Priority] asc, [System.CreatedDate] desc", area, asOfDate, wit);

            using (Task<HttpResponseMessage> response = connection.PostAsync("BPS.Scrum/_apis/wit/wiql?&api-version=1.0-preview.2",
                new KeyValuePair<string, string>("wiql", query)))
            {
                var workItems =
                    JsonConvert.DeserializeObject<QueryResults>(await response.Result.Content.ReadAsStringAsync());

                return workItems;
            }
        }
    }
}