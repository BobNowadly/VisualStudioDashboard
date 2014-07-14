﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dashboard.Data;
using Newtonsoft.Json;

namespace Dashboard.DataAccess
{
    public interface IWorkItemRepository
    {
        Task<QueryResults> GetInProcAndClosedWorkItems(string area);
        Task<IEnumerable<WorkItemUpdate>> GetWorkItemUpdates(int workItemId);
        Task<IEnumerable<WorkItemUpdate>> GetWorkItems(params int[] ids);
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
            string query = string.Format(@"Select [System.Id], [System.Title], [System.State]
                        From WorkItems 
                        Where [System.WorkItemType] = 'Product Backlog Item' 
                        AND [State] <> 'Removed' AND [State] EVER 'Closed' or [State] EVER 'Committed'
                        AND [Area Path] =  '{0}'
                        order by [Microsoft.VSTS.Common.Priority] asc, [System.CreatedDate] desc", area);

            using (Task<HttpResponseMessage> response = connection.PostAsync("queryresults?&api-version=1.0-preview",
                new KeyValuePair<string, string>("wiql", query)))
            {
                var workItems =
                    JsonConvert.DeserializeObject<QueryResults>(await response.Result.Content.ReadAsStringAsync());

                return workItems;
            }
        }

        public async Task<IEnumerable<WorkItemUpdate>> GetWorkItemUpdates(int workItemId)
        {
            string url = string.Format("workitems/{0}/updates?&api-version=1.0-preview", workItemId);
            using (Task<HttpResponseMessage> response = connection.GetAsync(url))
            {
                var updates =
                    JsonConvert.DeserializeObject<WorkItemUpdates>(
                        await response.Result.Content.ReadAsStringAsync());

                return updates.Value;
            }
        }

        public async Task<IEnumerable<WorkItemUpdate>> GetWorkItems(params int[] ids)
        {
            string idString = string.Join(",", ids);
            string url =
                string.Format(
                    "https://dodsongroupinc.visualstudio.com/defaultcollection/_apis/wit/workitems?ids={0}&api-version=1.0-preview",
                    idString);
            using (Task<HttpResponseMessage> response = connection.GetAsync(url))
            {
                var workItems =
                    JsonConvert.DeserializeObject<WorkItemUpdates>(
                        await response.Result.Content.ReadAsStringAsync());

                return workItems.Value;
            }
        }
    }
}