using System;
using System.Collections.Generic;
using System.Linq;
using Dashboard.Data;
using Dashboard.DataAccess;

namespace Dashboard
{
    public class Historian
    {
        private readonly IWorkItemRepository repository;

        public Historian(IWorkItemRepository repository)
        {
            this.repository = repository;
        }

        public List<WorkItem> GetCommittedAndClosedWorkItems()
        {
            // Get Workitem ids
            // to do move the area to the constructor 
            int[] workitemids =
                repository.GetInProcAndClosedWorkItems(@"BPS.Scrum\Dev -SEP Project")
                    .Result.Results.Select(s => s.SourceId)
                    .ToArray();

            // Get Work Items 
            IEnumerable<WorkItemUpdate> workitems = repository.GetWorkItems(workitemids).Result;

            // Get Get Changes Find Revision for Committed 
            var wi = new List<WorkItem>();
            foreach (WorkItemUpdate workitem in workitems)
            {
                IOrderedEnumerable<WorkItemUpdate> changes =
                    repository.GetWorkItemUpdates(workitem.Id)
                        .Result.OrderByDescending(a => DateTime.Parse(a.ChangedDate));
                WorkItemUpdate latestPreClosedState =
                    changes.FirstOrDefault(s => s.State != "Done" && !string.IsNullOrEmpty(s.State));
                    //Give me the state that happened before closed 

                DateTime? dateCommitted = latestPreClosedState != null &&
                                          !string.IsNullOrEmpty(latestPreClosedState.ChangedDate) &&
                                          latestPreClosedState.State == "Committed"
                    ? DateTime.Parse(latestPreClosedState.ChangedDate)
                    : (DateTime?) null;
                WorkItemUpdate latestStateChange = changes.FirstOrDefault(s => !string.IsNullOrEmpty(s.State));
                    // this should be the last state change
                DateTime? closedDAte = latestStateChange != null && !string.IsNullOrEmpty(latestStateChange.ClosedDate)
                    ? DateTime.Parse(latestStateChange.ClosedDate)
                    : (DateTime?) null;
                wi.Add(new WorkItem
                {
                    DateCommittedTime = dateCommitted,
                    DateClosed = closedDAte,
                    Id = workitem.Id,
                    Effort = int.Parse(workitem.Effort),
                    Title = workitem.Title
                });
            }

            return wi;
        }
    }
}